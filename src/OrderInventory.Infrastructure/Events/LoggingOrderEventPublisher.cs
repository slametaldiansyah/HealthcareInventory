using Microsoft.Extensions.Logging;
using OrderInventory.Application.Common.Interfaces;

namespace OrderInventory.Infrastructure.Events;

public class LoggingOrderEventPublisher : IOrderEventPublisher
{
    private readonly ILogger<LoggingOrderEventPublisher> _logger;

    public LoggingOrderEventPublisher(ILogger<LoggingOrderEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishOrderPaidAsync(
        Guid orderId,
        string paymentExternalId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "EVENT OrderPaid {{ OrderId: {OrderId}, PaymentExternalId: {PaymentExternalId}, At: {At:o} }}",
            orderId,
            paymentExternalId,
            DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public Task PublishOrderCancelledAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "EVENT OrderCancelled {{ OrderId: {OrderId}, At: {At:o} }}",
            orderId,
            DateTime.UtcNow);
        return Task.CompletedTask;
    }
}
