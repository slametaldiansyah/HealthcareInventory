using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OrderInventory.Application.DTOs;

namespace OrderInventory.IntegrationTests;

public class OrderInventoryApiTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Server=localhost,1433;Database=OrderInventoryDb_Test;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private OrderInventoryWebApplicationFactory? _factory;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        _factory = new OrderInventoryWebApplicationFactory(ConnectionString);
        _client = _factory.CreateClient();
        await ResetAsync();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    private async Task ResetAsync(params (string Sku, int Actual, int Reserved)[] inventory)
    {
        await _factory!.ResetDatabaseAsync();
        if (inventory.Length == 0)
        {
            await _factory.SeedInventoryAsync(("A1", 10, 0), ("B2", 10, 0));
        }
        else
        {
            await _factory.SeedInventoryAsync(inventory);
        }
    }

    [Fact]
    public async Task TC1_CreateOrder_SingleItem_ReservesStock()
    {
        await ResetAsync(("A1", 10, 0));

        var response = await _client!.PostAsJsonAsync("/orders", new CreateOrderRequestDto
        {
            UserId = Guid.NewGuid(),
            Items = [new OrderItemRequestDto("A1", 2)]
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);
        order!.Status.Should().Be("PLACED");
        order.Items.Should().ContainSingle(i => i.Sku == "A1" && i.Qty == 2);

        var inventory = await _client.GetFromJsonAsync<InventoryResponseDto>("/inventory/A1", JsonOptions);
        inventory!.ActualQty.Should().Be(10);
        inventory.ReservedQty.Should().Be(2);
    }

    [Fact]
    public async Task TC2_CreateOrder_MultiItem_InsufficientStock_NoPartialReserve()
    {
        await ResetAsync(("A1", 5, 0), ("B2", 10, 0));

        var response = await _client!.PostAsJsonAsync("/orders", new CreateOrderRequestDto
        {
            UserId = Guid.NewGuid(),
            Items =
            [
                new OrderItemRequestDto("A1", 3),
                new OrderItemRequestDto("B2", 20)
            ]
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("B2");
        body.Should().Contain("insufficient");

        var a1 = await _client.GetFromJsonAsync<InventoryResponseDto>("/inventory/A1", JsonOptions);
        var b2 = await _client.GetFromJsonAsync<InventoryResponseDto>("/inventory/B2", JsonOptions);
        a1!.ReservedQty.Should().Be(0);
        b2!.ReservedQty.Should().Be(0);
    }

    [Fact]
    public async Task TC3_Pay_IsIdempotent_ByPaymentExternalId()
    {
        await ResetAsync(("A1", 10, 0));
        var create = await _client!.PostAsJsonAsync("/orders", new CreateOrderRequestDto
        {
            UserId = Guid.NewGuid(),
            Items = [new OrderItemRequestDto("A1", 1)]
        });
        var order = await create.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);

        var payRequest = new PayOrderRequestDto { PaymentExternalId = "XYZ-123" };
        var first = await _client.PostAsJsonAsync($"/orders/{order!.Id}/pay", payRequest);
        var second = await _client.PostAsJsonAsync($"/orders/{order.Id}/pay", payRequest);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstBody = await first.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);
        var secondBody = await second.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);
        firstBody!.Status.Should().Be("PAID");
        firstBody.IdempotentReplay.Should().BeFalse();
        secondBody!.Status.Should().Be("PAID");
        secondBody.IdempotentReplay.Should().BeTrue();

        _factory!.EventPublisher.Events.Count(e => e.StartsWith($"OrderPaid:{order.Id}:")).Should().Be(1);

        var inventory = await _client.GetFromJsonAsync<InventoryResponseDto>("/inventory/A1", JsonOptions);
        inventory!.ActualQty.Should().Be(9);
        inventory.ReservedQty.Should().Be(0);
    }

    [Fact]
    public async Task TC4_Cancel_BeforeShip_ReleasesReservedStock()
    {
        await ResetAsync(("A1", 10, 0));
        var create = await _client!.PostAsJsonAsync("/orders", new CreateOrderRequestDto
        {
            UserId = Guid.NewGuid(),
            Items = [new OrderItemRequestDto("A1", 2)]
        });
        var order = await create.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);

        var cancel = await _client.PostAsync($"/orders/{order!.Id}/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelled = await cancel.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);
        cancelled!.Status.Should().Be("CANCELLED");

        var inventory = await _client.GetFromJsonAsync<InventoryResponseDto>("/inventory/A1", JsonOptions);
        inventory!.ActualQty.Should().Be(10);
        inventory.ReservedQty.Should().Be(0);
        _factory!.EventPublisher.Events.Should().Contain($"OrderCancelled:{order.Id}");
    }

    [Fact]
    public async Task TC5_Cancel_AfterPaid_ReturnsConflict()
    {
        await ResetAsync(("A1", 10, 0));
        var create = await _client!.PostAsJsonAsync("/orders", new CreateOrderRequestDto
        {
            UserId = Guid.NewGuid(),
            Items = [new OrderItemRequestDto("A1", 1)]
        });
        var order = await create.Content.ReadFromJsonAsync<OrderResponseDto>(JsonOptions);
        await _client.PostAsJsonAsync($"/orders/{order!.Id}/pay", new PayOrderRequestDto
        {
            PaymentExternalId = "PAY-1"
        });

        var cancel = await _client.PostAsync($"/orders/{order.Id}/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var inventory = await _client.GetFromJsonAsync<InventoryResponseDto>("/inventory/A1", JsonOptions);
        inventory!.ActualQty.Should().Be(9);
        inventory.ReservedQty.Should().Be(0);
    }

    [Fact]
    public async Task TC6_FlashSale_NoOverReservation()
    {
        await ResetAsync(("A1", 10, 0));
        var userId = Guid.NewGuid();

        var tasks = Enumerable.Range(0, 100).Select(_ =>
            _client!.PostAsJsonAsync("/orders", new CreateOrderRequestDto
            {
                UserId = userId,
                Items = [new OrderItemRequestDto("A1", 1)]
            })).ToArray();

        var responses = await Task.WhenAll(tasks);
        var success = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var failed = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        success.Should().Be(10);
        failed.Should().Be(90);

        var inventory = await _client!.GetFromJsonAsync<InventoryResponseDto>("/inventory/A1", JsonOptions);
        inventory!.ActualQty.Should().Be(10);
        inventory.ReservedQty.Should().Be(10);
        inventory.AvailableQty.Should().Be(0);
    }

    [Fact]
    public async Task TC7_GetInventory_ReturnsActualAndReserved()
    {
        await ResetAsync(("A1", 10, 0));
        await _client!.PostAsJsonAsync("/orders", new CreateOrderRequestDto
        {
            UserId = Guid.NewGuid(),
            Items = [new OrderItemRequestDto("A1", 3)]
        });

        var inventory = await _client.GetFromJsonAsync<InventoryResponseDto>("/inventory/A1", JsonOptions);
        inventory!.Sku.Should().Be("A1");
        inventory.ActualQty.Should().Be(10);
        inventory.ReservedQty.Should().Be(3);
        inventory.AvailableQty.Should().Be(7);
    }
}
