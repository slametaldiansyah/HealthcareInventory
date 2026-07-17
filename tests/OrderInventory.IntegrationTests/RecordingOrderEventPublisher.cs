using System.Collections.Concurrent;
using OrderInventory.Application.Common.Interfaces;

namespace OrderInventory.IntegrationTests;

public sealed class RecordingOrderEventPublisher : IOrderEventPublisher
{
    private readonly ConcurrentBag<string> _events = new();

    public IReadOnlyCollection<string> Events => _events;

    public Task PublishOrderPaidAsync(
        Guid orderId,
        string paymentExternalId,
        CancellationToken cancellationToken = default)
    {
        _events.Add($"OrderPaid:{orderId}:{paymentExternalId}");
        return Task.CompletedTask;
    }

    public Task PublishOrderCancelledAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _events.Add($"OrderCancelled:{orderId}");
        return Task.CompletedTask;
    }

    public void Clear() => _events.Clear();
}
