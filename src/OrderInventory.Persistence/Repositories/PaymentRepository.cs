using Microsoft.EntityFrameworkCore;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Interfaces;
using OrderInventory.Persistence.Context;

namespace OrderInventory.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly OrderInventoryDbContext _context;

    public PaymentRepository(OrderInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByExternalIdAsync(
        string paymentExternalId,
        CancellationToken cancellationToken = default) =>
        await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentExternalId == paymentExternalId, cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>
        await _context.Payments.AddAsync(payment, cancellationToken);
}
