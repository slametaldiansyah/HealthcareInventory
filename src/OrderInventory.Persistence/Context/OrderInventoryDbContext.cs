using OrderInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderInventory.Persistence.Context;

public class OrderInventoryDbContext : DbContext
{
    public OrderInventoryDbContext(DbContextOptions<OrderInventoryDbContext> options) : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderInventoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
