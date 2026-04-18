using Microsoft.AspNetCore.Identity;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Constants;

namespace MultiVendorECommerce.Infrastructure.Seeds;

public static class SeedAdminAsync
{
    public static async Task SeedAsync(UserManager<User> userManager)
    {
        const string adminEmail = "admin@multivendor.com";
        const string adminPassword = "Admin@12345";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new User
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);

        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}
