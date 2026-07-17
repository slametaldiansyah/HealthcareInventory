namespace OrderInventory.Application.DTOs;

public sealed record OrderItemRequestDto(string Sku, int Qty);

public sealed class CreateOrderRequestDto
{
    public Guid UserId { get; set; }
    public List<OrderItemRequestDto> Items { get; set; } = [];
}

public sealed class PayOrderRequestDto
{
    public string PaymentExternalId { get; set; } = string.Empty;
}

public sealed record OrderItemDto(string Sku, int Qty);

public sealed class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? PaymentExternalId { get; set; }
    public bool IdempotentReplay { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public sealed class InventoryResponseDto
{
    public string Sku { get; set; } = string.Empty;
    public int ActualQty { get; set; }
    public int ReservedQty { get; set; }
    public int AvailableQty { get; set; }
}
