using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class PaymentConfigurations : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment", "order");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("Id").ValueGeneratedOnAdd().UseIdentityAlwaysColumn();
        builder.Property(p => p.OrderId).HasColumnName("OrderId").IsRequired();
        builder.Property(p => p.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.Provider).HasColumnName("Provider").HasMaxLength(100);
        builder.Property(p => p.Status).HasColumnName("Status").HasDefaultValue(PaymentStatus.Pending);
        builder.Property(p => p.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.ModifiedAt).HasColumnName("ModifiedAt");

        // Indexes
        builder.HasIndex(p => p.OrderId).IsUnique().HasDatabaseName("IX_Payment_OrderId");
        builder.HasIndex(p => p.Status).HasDatabaseName("IX_Payment_Status");

        // Relationships
        builder.HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
