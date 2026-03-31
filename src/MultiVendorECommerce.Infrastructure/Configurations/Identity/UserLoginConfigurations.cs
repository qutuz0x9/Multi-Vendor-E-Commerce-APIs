
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Configurations.Identity;

public class UserLoginConfigurations : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable("UserLogin", "identity");

        builder.Property(ul => ul.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
