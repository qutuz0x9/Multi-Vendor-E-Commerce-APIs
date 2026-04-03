using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class InventoryConfigurations : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventory", "inventory");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("Id").ValueGeneratedOnAdd().UseIdentityAlwaysColumn();
        builder.Property(i => i.Quantity).HasColumnName("Quantity").IsRequired().HasDefaultValue(0);
        builder.Property(i => i.ReservedQuantity).HasColumnName("ReservedQuantity").IsRequired().HasDefaultValue(0);
        builder.Property(i => i.Status).HasColumnName("Status").HasDefaultValue(InventoryStatus.Available);
        builder.Property(i => i.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(i => i.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(i => i.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        builder.Property(i => i.DeletedAt).HasColumnName("DeletedAt");

        // Indexes
        builder.HasIndex(i => i.VendorOfferId).HasDatabaseName("IX_Inventory_VendorOfferId");
        builder.HasIndex(i => i.IsDeleted).HasDatabaseName("IX_Inventory_IsDeleted");

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(i => !i.IsDeleted);

        // Relationships
        builder.HasOne(i => i.VendorOffer)
               .WithOne(vo => vo.Inventory)
               .HasForeignKey<Inventory>(i => i.VendorOfferId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
