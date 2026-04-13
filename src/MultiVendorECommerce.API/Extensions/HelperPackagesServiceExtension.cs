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
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssemblyContaining<LoginRequestDTO>();
        return services;
    }
}