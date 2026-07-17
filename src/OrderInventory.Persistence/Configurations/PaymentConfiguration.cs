using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderInventory.Domain.Entities;

namespace OrderInventory.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PaymentExternalId).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.PaymentExternalId).IsUnique();
        builder.HasIndex(x => x.OrderId).IsUnique();
    }
}
