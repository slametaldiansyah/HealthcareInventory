namespace OrderInventory.Domain.Exceptions;

public class InsufficientStockException : Exception
{
    public IReadOnlyList<InsufficientStockDetail> Details { get; }

    public InsufficientStockException(IReadOnlyList<InsufficientStockDetail> details)
        : base(BuildMessage(details))
    {
        Details = details;
    }

    private static string BuildMessage(IReadOnlyList<InsufficientStockDetail> details) =>
        string.Join("; ", details.Select(d =>
            $"{d.Sku} insufficient (requested={d.Requested}, available={d.Available})"));
}

public sealed record InsufficientStockDetail(string Sku, int Requested, int Available);
