using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class OrderConfigurations : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order", "order");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("Id").ValueGeneratedOnAdd().UseIdentityAlwaysColumn();
        builder.Property(o => o.CustomerId).HasColumnName("CustomerId").IsRequired();
        builder.Property(o => o.TotalAmount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(o => o.Status).HasColumnName("Status").HasDefaultValue(OrderStatus.Pending);
        builder.Property(o => o.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(o => o.ModifiedAt).HasColumnName("ModifiedAt");

        // Indexes
        builder.HasIndex(o => o.CustomerId).HasDatabaseName("IX_Order_CustomerId");
        builder.HasIndex(o => o.Status).HasDatabaseName("IX_Order_Status");

        // Relationships
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.ShippingAddress)
            .WithOne(sa => sa.Order)
            .HasForeignKey<OrderShippingAddress>(sa => sa.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
