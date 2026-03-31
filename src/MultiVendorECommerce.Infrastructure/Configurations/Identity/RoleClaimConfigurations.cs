using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations.Identity;

public class RoleClaimConfigurations : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable("RoleClaim", "identity");
        builder.Property(rc => rc.CreatedAt).
            HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
