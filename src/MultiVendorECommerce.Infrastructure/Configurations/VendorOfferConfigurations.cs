
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class VendorOfferConfigurations : IEntityTypeConfiguration<VendorOffer>
{
    public void Configure(EntityTypeBuilder<VendorOffer> builder)
    {
        builder.ToTable("VendorOffer", "inventory");
        builder.HasKey(vo => vo.Id);
        builder.Property(vo => vo.Id).HasColumnName("Id").UseIdentityAlwaysColumn().ValueGeneratedOnAdd();
        builder.Property(vo => vo.VendorId).HasColumnName("VendorId").IsRequired();
        builder.Property(vo => vo.ProductId).HasColumnName("ProductId").IsRequired();
        builder.Property(vo => vo.Price).HasColumnName("Price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(vo => vo.Staus).HasColumnName("Staus").HasDefaultValue(VendorOfferStatus.Active);
        builder.Property(vo => vo.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(vo => vo.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(vo => vo.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);


        // Indexes
        builder.HasIndex(vo => new { vo.VendorId, vo.ProductId }).IsUnique();
        builder.HasIndex(vo => vo.IsDeleted).HasDatabaseName("IX_VendorOffer_IsDeleted");


        // Global Query Filter for Soft Delete

        // Relationships
        builder.HasOne(vo => vo.Product)
            .WithMany(p => p.VendorOffers)
            .HasForeignKey(vo => vo.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vo => vo.Vendor)
            .WithMany(v => v.VendorOffers)
            .HasForeignKey(vo => vo.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vo => vo.CartItem)
            .WithOne(ci => ci.VendorOffer)
            .HasForeignKey<CartItem>(ci => ci.VendorOfferId);
    }
}
