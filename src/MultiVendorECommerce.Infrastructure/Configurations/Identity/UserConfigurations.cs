using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations.Identity;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", "identity");

        builder.Property(u => u.Status).HasDefaultValue(UserStatus.Active);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(u => u.IsDeleted).HasDatabaseName("IX_User_IsDeleted");

    }
}
