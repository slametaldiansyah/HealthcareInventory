namespace OrderInventory.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string PaymentExternalId { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; }

    public Order Order { get; set; } = null!;
}
