using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class CustomerAddressConfigurations : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddress", "customer");
        builder.HasKey(ca => ca.Id);
        builder.Property(ca => ca.Id).HasColumnName("Id").ValueGeneratedOnAdd().UseIdentityAlwaysColumn();
        builder.Property(ca => ca.CustomerId).HasColumnName("CustomerId").IsRequired();
        builder.Property(ca => ca.Address).HasColumnName("Address").HasMaxLength(500).IsRequired();
        builder.Property(ca => ca.City).HasColumnName("City").HasMaxLength(100).IsRequired();
        builder.Property(ca => ca.Country).HasColumnName("Country").HasMaxLength(100).IsRequired();
        builder.Property(ca => ca.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
        builder.Property(ca => ca.AddressType).HasColumnName("AddressType").IsRequired();
        builder.Property(ca => ca.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(ca => ca.ModifiedAt).HasColumnName("ModifiedAt");

        // Indexes
        builder.HasIndex(ca => ca.CustomerId).HasDatabaseName("IX_CustomerAddress_CustomerId");
        builder.HasIndex(ca => ca.AddressType).HasDatabaseName("IX_CustomerAddress_AddressType");

        // Relationships
        builder.HasOne(ca => ca.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
