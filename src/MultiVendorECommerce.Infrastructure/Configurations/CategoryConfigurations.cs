using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class CategoryConfigurations : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category", "product");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("Id").UseIdentityAlwaysColumn().ValueGeneratedOnAdd();
        builder.Property(c => c.Name).HasColumnName("Name").IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasColumnName("Description").HasMaxLength(500);
        builder.Property(c => c.Slug).HasColumnName("Slug").HasMaxLength(150).IsRequired();
        builder.Property(c => c.Status).HasColumnName("Status").HasDefaultValue(CategoryStatus.Active);
        builder.Property(c => c.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(c => c.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        builder.Property(c => c.DeletedAt).HasColumnName("DeletedAt");

        // Indexes
        builder.HasIndex(p => p.Name).HasDatabaseName("IX_Category_Name");
        builder.HasIndex(p => p.Slug).HasDatabaseName("IX_Category_Slug").IsUnique();
        builder.HasIndex(p => p.IsDeleted).HasDatabaseName("IX_Category_IsDeleted");

        // Global query filter for soft delete

        // Relationships
        builder.HasMany(c => c.ProductCategories)
            .WithOne(pc => pc.Category)
            .HasForeignKey(pc => pc.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}
