
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class VendorConfigurations : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendor", "vendor");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("Id")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();
        builder.Property(v => v.Slug)
            .HasColumnName("Slug")
            .HasMaxLength(255)
            .IsRequired(false);


        builder.Property(v => v.BusinessName)
            .HasColumnName("BusinessName")
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(v => v.WebsiteUrl)
            .HasColumnName("WebsiteUrl")
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(v => v.Status)
            .HasColumnType("vendor_status")
            .HasDefaultValue(VendorStatus.Pending);
        builder.Property(v => v.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(v => v.ModifiedAt)
            .HasColumnName("ModifiedAt");
        builder.Property(v => v.IsDeleted)
            .HasColumnName("IsDeleted")
            .HasDefaultValue(false);
        builder.Property(v => v.DeletedAt)
            .HasColumnName("DeletedAt");
        builder.Property(v => v.AverageRate)
            .HasColumnName("AverageRate")
            .HasColumnType("decimal(3,2)");
        builder.Property(v => v.TotalReviews)
            .HasColumnName("TotalReviews")
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(v => v.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Vendor_Slug");
        builder.HasIndex(v => v.BusinessName)
            .IsUnique().HasDatabaseName("IX_Vendor_BusinessName");
        builder.HasIndex(v => v.WebsiteUrl)
            .IsUnique().HasDatabaseName("IX_Vendor_WebsiteUrl");
        builder.HasIndex(v => v.IsDeleted).HasDatabaseName("IX_Vendor_IsDeleted");

        // Relationships
        builder.HasOne(v => v.User)
            .WithOne(u => u.Vendor)
            .HasForeignKey<Vendor>(v => v.Id);

    }
}
