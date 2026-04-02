
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class BrandConfigurations : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brand", "product");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("Id").UseIdentityAlwaysColumn().ValueGeneratedOnAdd();
        builder.Property(b => b.Name).HasColumnName("Name").HasMaxLength(255).IsRequired();
        builder.Property(b => b.NormalizedName).HasColumnName("NormalizedName").HasMaxLength(255).IsRequired();
        builder.Property(b => b.Status).HasColumnName("brand_status").HasDefaultValue(BrandStatus.Active);
        builder.Property(b => b.Slug).HasColumnName("Slug").HasMaxLength(255).IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(b => b.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        builder.Property(b => b.DeletedAt).HasColumnName("DeletedAt");

        // Indexes
        builder.HasIndex(b => b.NormalizedName).HasDatabaseName("IX_Brand_NormalizedName").IsUnique();
        builder.HasIndex(b => b.Name).HasDatabaseName("IX_Brand_Name").IsUnique();
        builder.HasIndex(b => b.Slug).HasDatabaseName("IX_Brand_Slug").IsUnique();
        builder.HasIndex(b => b.IsDeleted).HasDatabaseName("IX_Brand_IsDeleted");

        // Global query filter to exclude soft-deleted records

        // Relationships
        builder.HasMany(b => b.Products)
            .WithOne(p => p.Brand)
            .HasForeignKey(b => b.BrandId);
    }
}
