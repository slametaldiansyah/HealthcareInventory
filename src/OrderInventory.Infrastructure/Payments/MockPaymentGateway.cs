using Microsoft.Extensions.Logging;
using OrderInventory.Application.Common.Interfaces;

namespace OrderInventory.Infrastructure.Payments;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<bool> ChargeAsync(
        Guid orderId,
        string paymentExternalId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Mock payment gateway charged OrderId={OrderId} PaymentExternalId={PaymentExternalId}",
            orderId,
            paymentExternalId);
        return Task.FromResult(true);
    }
}
