using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class ProductCategoryConfigurations : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategory", "product");
        builder.HasKey(pc => pc.Id);
        builder.Property(pc => pc.Id).HasColumnName("Id").UseIdentityAlwaysColumn().ValueGeneratedOnAdd();
        builder.Property(pc => pc.ProductId).HasColumnName("ProductId").IsRequired();
        builder.Property(pc => pc.CategoryId).HasColumnName("CategoryId").IsRequired();
        builder.Property(pc => pc.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(pc => pc.ModifiedAt).HasColumnName("ModifiedAt");

        // Indexes
        builder.HasIndex(pc => pc.ProductId).HasDatabaseName("IX_ProductCategory_ProductId");
        builder.HasIndex(pc => pc.CategoryId).HasDatabaseName("IX_ProductCategory_CategoryId");
        builder.HasIndex(pc => new { pc.ProductId, pc.CategoryId }).HasDatabaseName("IX_ProductCategory_ProductId_CategoryId").IsUnique();

        // Relationships
        builder.HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
