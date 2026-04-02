
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class ProductConfigurations : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product", "product");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("Id").UseIdentityAlwaysColumn().ValueGeneratedOnAdd();
        builder.Property(p => p.Name).HasColumnName("Name").HasMaxLength(255).IsRequired();
        builder.Property(p => p.Description).HasColumnName("Description").HasMaxLength(1000);
        builder.Property(p => p.Status).HasColumnName("Status").HasDefaultValue(ProductStatus.Active);
        builder.Property(p => p.Slug).HasColumnName("Slug").HasMaxLength(255).IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(p => p.DeletedAt).HasColumnName("DeletedAt");
        builder.Property(p => p.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

        // Indexes
        builder.HasIndex(p => p.Name).HasDatabaseName("IX_Product_Name");
        builder.HasIndex(p => p.Slug).HasDatabaseName("IX_Product_Slug").IsUnique();
        builder.HasIndex(p => p.BrandId).HasDatabaseName("IX_Product_BrandId");
        builder.HasIndex(p => p.IsDeleted).HasDatabaseName("IX_Product_IsDeleted");

        // Global Query Filter for soft delete


        // Relationships
        builder.HasOne(p => p.Brand)
            .WithMany(p => p.Products)
            .HasForeignKey(p => p.BrandId);

        builder.HasMany(p => p.ProductCategories)
            .WithOne(pc => pc.Product)
            .HasForeignKey(pc => pc.ProductId).OnDelete(DeleteBehavior.Cascade);


    }
}
