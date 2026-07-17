namespace OrderInventory.Application.Common.Interfaces;

public interface IOrderEventPublisher
{
    Task PublishOrderPaidAsync(Guid orderId, string paymentExternalId, CancellationToken cancellationToken = default);

    Task PublishOrderCancelledAsync(Guid orderId, CancellationToken cancellationToken = default);
}
