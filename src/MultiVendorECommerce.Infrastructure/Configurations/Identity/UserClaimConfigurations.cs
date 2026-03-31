using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations.Identity;

public class UserClaimConfigurations : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable("UserClaim", "identity");

        builder.Property(uc => uc.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
