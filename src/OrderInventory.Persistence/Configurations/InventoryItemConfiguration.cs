using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderInventory.Domain.Entities;

namespace OrderInventory.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems", t =>
        {
            t.HasCheckConstraint("CK_InventoryItems_Reserved_NonNegative", "[ReservedQty] >= 0");
            t.HasCheckConstraint("CK_InventoryItems_Actual_NonNegative", "[ActualQty] >= 0");
            t.HasCheckConstraint("CK_InventoryItems_Reserved_Lte_Actual", "[ReservedQty] <= [ActualQty]");
        });

        builder.HasKey(x => x.Sku);
        builder.Property(x => x.Sku).HasMaxLength(64);
        builder.Property(x => x.ActualQty).IsRequired();
        builder.Property(x => x.ReservedQty).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Ignore(x => x.AvailableQty);
    }
}
