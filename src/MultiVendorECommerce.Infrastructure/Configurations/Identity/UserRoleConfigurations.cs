using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations.Identity;

public class UserRoleConfigurations : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole", "identity");
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        builder.Property(ur => ur.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

    }
}
