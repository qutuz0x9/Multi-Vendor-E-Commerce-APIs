using System.Reflection;
using MultiVendorECommerce.Application.Profiles;
using FluentValidation;
using MultiVendorECommerce.Application.DTOs.Auth;

namespace MultiVendorECommerce.API.Extensions;

public static class HelperPackagesServiceExtension
{
    public static IServiceCollection AddHelperPackages(this IServiceCollection services)
    {
        // 1) AutoMapper
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile(new UserProfile());
            cfg.AddProfile(new BrandProfile());
            cfg.AddProfile(new CategoryProfile());
            cfg.AddProfile(new ProductProfile());
            cfg.AddProfile(new ProductCategoryProfile());
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssemblyContaining<LoginRequestDTO>();
        return services;
    }
}