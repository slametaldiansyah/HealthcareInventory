using OrderInventory.Domain.Entities;

namespace OrderInventory.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, InventoryItem>> GetBySkusForUpdateAsync(
        IEnumerable<string> skus,
        CancellationToken cancellationToken = default);

    Task<bool> TryReserveAsync(string sku, int qty, CancellationToken cancellationToken = default);

    Task<bool> TryCommitReservationAsync(string sku, int qty, CancellationToken cancellationToken = default);

    Task<bool> TryReleaseReservationAsync(string sku, int qty, CancellationToken cancellationToken = default);

    Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default);
}
