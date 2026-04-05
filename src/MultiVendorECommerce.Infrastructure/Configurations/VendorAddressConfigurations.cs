using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class VendorAddressConfigurations : IEntityTypeConfiguration<VendorAddress>
{
    public void Configure(EntityTypeBuilder<VendorAddress> builder)
    {
        builder.ToTable("VendorAddress", "vendor");
        builder.HasKey(va => va.Id);
        builder.Property(va => va.Id).HasColumnName("Id").ValueGeneratedOnAdd().UseIdentityAlwaysColumn();
        builder.Property(va => va.VendorId).HasColumnName("VendorId").IsRequired();
        builder.Property(va => va.Address).HasColumnName("Address").HasMaxLength(500).IsRequired();
        builder.Property(va => va.City).HasColumnName("City").HasMaxLength(100).IsRequired();
        builder.Property(va => va.Country).HasColumnName("Country").HasMaxLength(100).IsRequired();
        builder.Property(va => va.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
        builder.Property(va => va.AddressType).HasColumnName("AddressType").IsRequired();
        builder.Property(va => va.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(va => va.ModifiedAt).HasColumnName("ModifiedAt");

        // Indexes
        builder.HasIndex(va => va.VendorId).HasDatabaseName("IX_VendorAddress_VendorId");
        builder.HasIndex(va => va.AddressType).HasDatabaseName("IX_VendorAddress_AddressType");

        // Relationships
        builder.HasOne(va => va.Vendor)
            .WithMany(v => v.VendorAddresses)
            .HasForeignKey(va => va.VendorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
