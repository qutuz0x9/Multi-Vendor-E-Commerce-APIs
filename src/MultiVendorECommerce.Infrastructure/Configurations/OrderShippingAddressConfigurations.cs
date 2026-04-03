using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class OrderShippingAddressConfigurations : IEntityTypeConfiguration<OrderShippingAddress>
{
    public void Configure(EntityTypeBuilder<OrderShippingAddress> builder)
    {
        builder.ToTable("OrderShippingAddress", "order");
        builder.HasKey(osa => osa.Id);
        builder.Property(osa => osa.Id).HasColumnName("Id").ValueGeneratedOnAdd().UseIdentityAlwaysColumn();
        builder.Property(osa => osa.OrderId).HasColumnName("OrderId").IsRequired();
        builder.Property(osa => osa.ShippingAddress).HasColumnName("ShippingAddress").HasMaxLength(500).IsRequired();
        builder.Property(osa => osa.ShippingCity).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
        builder.Property(osa => osa.ShippingCountry).HasColumnName("ShippingCountry").HasMaxLength(100).IsRequired();
        builder.Property(osa => osa.ShippingPhoneNumber).HasColumnName("ShippingPhoneNumber").HasMaxLength(20).IsRequired();
        builder.Property(osa => osa.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(osa => osa.ModifiedAt).HasColumnName("ModifiedAt");

        // Indexes
        builder.HasIndex(osa => osa.OrderId).IsUnique().HasDatabaseName("IX_OrderShippingAddress_OrderId");

        // Relationships
        builder.HasOne(osa => osa.Order)
            .WithOne(o => o.ShippingAddress)
            .HasForeignKey<OrderShippingAddress>(osa => osa.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
