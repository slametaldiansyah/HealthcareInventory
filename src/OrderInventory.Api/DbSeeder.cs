using Microsoft.EntityFrameworkCore;
using OrderInventory.Domain.Entities;
using OrderInventory.Persistence.Context;

namespace OrderInventory.Api;

public static class DbSeeder
{
    public const string SkuA1 = "A1";
    public const string SkuB2 = "B2";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderInventoryDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.InventoryItems.AnyAsync())
        {
            db.InventoryItems.AddRange(
                new InventoryItem { Sku = SkuA1, ActualQty = 10, ReservedQty = 0 },
                new InventoryItem { Sku = SkuB2, ActualQty = 10, ReservedQty = 0 });
            await db.SaveChangesAsync();
        }
    }

    public static async Task ResetInventoryAsync(
        IServiceProvider services,
        string sku,
        int actualQty,
        int reservedQty = 0)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderInventoryDbContext>();
        var item = await db.InventoryItems.FirstOrDefaultAsync(i => i.Sku == sku);
        if (item is null)
        {
            db.InventoryItems.Add(new InventoryItem
            {
                Sku = sku,
                ActualQty = actualQty,
                ReservedQty = reservedQty
            });
        }
        else
        {
            item.ActualQty = actualQty;
            item.ReservedQty = reservedQty;
        }

        await db.SaveChangesAsync();
    }
}
