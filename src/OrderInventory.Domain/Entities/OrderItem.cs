namespace OrderInventory.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Qty { get; set; }

    public Order Order { get; set; } = null!;
}
