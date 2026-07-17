namespace OrderInventory.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<bool> ChargeAsync(Guid orderId, string paymentExternalId, CancellationToken cancellationToken = default);
}
