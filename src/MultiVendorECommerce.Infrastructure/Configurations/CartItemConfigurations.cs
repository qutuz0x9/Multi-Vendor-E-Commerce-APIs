
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class CartItemConfigurations : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItem", "cart");
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.Id).HasColumnName("Id").ValueGeneratedOnAdd().UseIdentityAlwaysColumn();
        builder.Property(ci => ci.Quantity).HasColumnName("Quantity").IsRequired().HasDefaultValue(0);
        builder.Property(ci => ci.CartSessionId).HasColumnName("CartSessionId").IsRequired();
        builder.Property(ci => ci.VendorOfferId).HasColumnName("VendorOfferId").IsRequired();
        builder.Property(ci => ci.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(ci => ci.ModifiedAt).HasColumnName("ModifiedAt");


        // Indexes
        builder.HasIndex(ci => ci.CreatedAt).HasDatabaseName("IX_CartItem_CreatedAt");
        builder.HasIndex(ci => ci.VendorOfferId).HasDatabaseName("IX_CartItem_VendorOfferId");

        // Relationships
        builder.HasOne(ci => ci.CartSession)
            .WithMany(cs => cs.CartItems)
            .HasForeignKey(ci => ci.Id);

        builder.HasOne(ci => ci.VendorOffer)
            .WithOne(vo => vo.CartItem)
            .HasForeignKey<CartItem>(ci => ci.VendorOfferId);


    }
}
