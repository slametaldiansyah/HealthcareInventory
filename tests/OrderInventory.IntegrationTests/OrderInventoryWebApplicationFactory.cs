using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderInventory.Application.Common.Interfaces;
using OrderInventory.Domain.Entities;
using OrderInventory.Persistence.Context;

namespace OrderInventory.IntegrationTests;

public class OrderInventoryWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    public RecordingOrderEventPublisher EventPublisher { get; } = new();

    public OrderInventoryWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<OrderInventoryDbContext>));
            services.RemoveAll(typeof(OrderInventoryDbContext));

            services.AddDbContext<OrderInventoryDbContext>(options =>
                options.UseSqlServer(_connectionString));

            services.RemoveAll<IOrderEventPublisher>();
            services.AddSingleton<IOrderEventPublisher>(EventPublisher);
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderInventoryDbContext>();
        await context.Database.MigrateAsync();
        await context.Payments.ExecuteDeleteAsync();
        await context.OrderItems.ExecuteDeleteAsync();
        await context.Orders.ExecuteDeleteAsync();
        await context.InventoryItems.ExecuteDeleteAsync();
        EventPublisher.Clear();
    }

    public async Task SeedInventoryAsync(params (string Sku, int Actual, int Reserved)[] items)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderInventoryDbContext>();
        foreach (var (sku, actual, reserved) in items)
        {
            context.InventoryItems.Add(new InventoryItem
            {
                Sku = sku,
                ActualQty = actual,
                ReservedQty = reserved
            });
        }

        await context.SaveChangesAsync();
    }
}
