using Microsoft.EntityFrameworkCore;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Interfaces;
using OrderInventory.Persistence.Context;

namespace OrderInventory.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly OrderInventoryDbContext _context;

    public InventoryRepository(OrderInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        await _context.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Sku == sku, cancellationToken);

    public async Task<IReadOnlyDictionary<string, InventoryItem>> GetBySkusForUpdateAsync(
        IEnumerable<string> skus,
        CancellationToken cancellationToken = default)
    {
        var orderedSkus = skus.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList();
        var result = new Dictionary<string, InventoryItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var sku in orderedSkus)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM InventoryItems WITH (UPDLOCK, ROWLOCK) WHERE Sku = {sku}",
                cancellationToken);

            var item = await _context.InventoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Sku == sku, cancellationToken);

            if (item is not null)
            {
                result[item.Sku] = item;
            }
        }

        return result;
    }

    public async Task<bool> TryReserveAsync(string sku, int qty, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE InventoryItems WITH (UPDLOCK, ROWLOCK)
             SET ReservedQty = ReservedQty + {qty}
             WHERE Sku = {sku}
               AND ActualQty - ReservedQty >= {qty}
             """,
            cancellationToken);

        return rows == 1;
    }

    public async Task<bool> TryCommitReservationAsync(string sku, int qty, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE InventoryItems WITH (UPDLOCK, ROWLOCK)
             SET ActualQty = ActualQty - {qty},
                 ReservedQty = ReservedQty - {qty}
             WHERE Sku = {sku}
               AND ReservedQty >= {qty}
               AND ActualQty >= {qty}
             """,
            cancellationToken);

        return rows == 1;
    }

    public async Task<bool> TryReleaseReservationAsync(string sku, int qty, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE InventoryItems WITH (UPDLOCK, ROWLOCK)
             SET ReservedQty = ReservedQty - {qty}
             WHERE Sku = {sku}
               AND ReservedQty >= {qty}
             """,
            cancellationToken);

        return rows == 1;
    }

    public async Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default) =>
        await _context.InventoryItems.AddAsync(item, cancellationToken);
}
