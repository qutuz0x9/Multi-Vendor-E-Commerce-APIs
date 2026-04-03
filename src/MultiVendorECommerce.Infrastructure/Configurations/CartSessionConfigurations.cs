using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class CartSessionConfigurations : IEntityTypeConfiguration<CartSession>
{
    public void Configure(EntityTypeBuilder<CartSession> builder)
    {
        builder.ToTable("CartSession", "cart");
        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.Id).HasColumnName("Id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(cs => cs.CustomerId).HasColumnName("CustomerId").IsRequired();
        builder.Property(cs => cs.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(cs => cs.ModifiedAt).HasColumnName("ModifiedAt");

        // Indexes
        builder.HasIndex(cs => cs.CustomerId).HasDatabaseName("IX_CartSession_CustomerId");

        // Relationships
        builder.HasOne(cs => cs.Customer)
            .WithOne(c => c.CartSession)
            .HasForeignKey<CartSession>(cs => cs.CustomerId);

        builder.HasMany(cs => cs.CartItems)
            .WithOne(ci => ci.CartSession)
            .HasForeignKey(ci => ci.CartSessionId);
    }
}
