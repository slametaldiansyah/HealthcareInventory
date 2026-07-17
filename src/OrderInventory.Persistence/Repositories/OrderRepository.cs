using Microsoft.EntityFrameworkCore;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Interfaces;
using OrderInventory.Persistence.Context;

namespace OrderInventory.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderInventoryDbContext _context;

    public OrderRepository(OrderInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

    public async Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await _context.Orders.AddAsync(order, cancellationToken);
}
