using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Infrastructure.Contexts;

public class ECommerceDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasPostgresEnum<UserStatus>("user_status");
        builder.HasPostgresEnum<VendorStatus>("vendor_status");
        builder.HasPostgresEnum<BrandStatus>("brand_status");
        builder.HasPostgresEnum<ProductStatus>("product_status");
        builder.HasPostgresEnum<CategoryStatus>("category_status");

        builder.ApplyConfigurationsFromAssembly(typeof(ECommerceDbContext).Assembly);
    }

}
