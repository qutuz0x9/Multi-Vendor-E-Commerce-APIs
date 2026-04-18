using Microsoft.AspNetCore.Identity;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Constants;

namespace MultiVendorECommerce.Infrastructure.Seeds;

public static class SeedRolesAsync
{
    public static async Task SeedAsync(RoleManager<Role> roleManager)
    {
        string[] roleNames = [Roles.Admin, Roles.Vendor, Roles.Customer];

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = roleName,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}
