using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations;

public class CustomerConfigurations : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customer", "customer");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("Id")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(c => c.UserId).HasColumnName("UserId").IsRequired();
        builder.Property(c => c.FirstName).HasColumnName("FirstName").HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasColumnName("LastName").HasMaxLength(100).IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(c => c.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        builder.Property(c => c.DeletedAt).HasColumnName("DeletedAt");

        builder.HasIndex(c => c.UserId).IsUnique().HasDatabaseName("IX_Customer_UserId");
        builder.HasIndex(c => c.IsDeleted).HasDatabaseName("IX_Customer_IsDeleted");


        // Relationships
        builder.HasOne(c => c.User)
            .WithOne(u => u.Customer)
            .HasForeignKey<Customer>(c => c.UserId);

    }
}
