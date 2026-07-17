namespace OrderInventory.Domain.Entities;

public class InventoryItem
{
    public string Sku { get; set; } = string.Empty;
    public int ActualQty { get; set; }
    public int ReservedQty { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public int AvailableQty => ActualQty - ReservedQty;

    public void Reserve(int qty)
    {
        if (qty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be positive.");
        }

        if (AvailableQty < qty)
        {
            throw new InvalidOperationException($"Insufficient available stock for SKU '{Sku}'.");
        }

        ReservedQty += qty;
    }

    public void CommitReservation(int qty)
    {
        if (qty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be positive.");
        }

        if (ReservedQty < qty || ActualQty < qty)
        {
            throw new InvalidOperationException($"Cannot commit reservation for SKU '{Sku}'.");
        }

        ReservedQty -= qty;
        ActualQty -= qty;
    }

    public void ReleaseReservation(int qty)
    {
        if (qty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be positive.");
        }

        if (ReservedQty < qty)
        {
            throw new InvalidOperationException($"Cannot release more than reserved for SKU '{Sku}'.");
        }

        ReservedQty -= qty;
    }
}
