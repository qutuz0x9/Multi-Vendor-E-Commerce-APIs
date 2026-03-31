using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations.Identity;

public class UserTokenConfigurations : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("UserToken", "identity");

        builder.Property(ut => ut.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
