using OrderInventory.Domain.Entities;

namespace OrderInventory.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByExternalIdAsync(string paymentExternalId, CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
