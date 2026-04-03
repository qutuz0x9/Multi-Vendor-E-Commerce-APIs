using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class OrderItemConfigurations : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItem", "order");
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id).HasColumnName("Id").ValueGeneratedOnAdd().UseIdentityAlwaysColumn();
        builder.Property(oi => oi.OrderId).HasColumnName("OrderId").IsRequired();
        builder.Property(oi => oi.VendorOfferId).HasColumnName("VendorOfferId").IsRequired();
        builder.Property(oi => oi.ProductName).HasColumnName("ProductName").HasMaxLength(255);
        builder.Property(oi => oi.Quantity).HasColumnName("Quantity").IsRequired().HasDefaultValue(0);
        builder.Property(oi => oi.UnitPrice).HasColumnName("UnitPrice").HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.Price).HasColumnName("Price").HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(oi => oi.ModifiedAt).HasColumnName("ModifiedAt");

        // Indexes
        builder.HasIndex(oi => oi.OrderId).HasDatabaseName("IX_OrderItem_OrderId");
        builder.HasIndex(oi => oi.VendorOfferId).HasDatabaseName("IX_OrderItem_VendorOfferId");

        // Relationships
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.VendorOffer)
            .WithOne(vo => vo.OrderItem)
            .HasForeignKey<OrderItem>(oi => oi.VendorOfferId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
