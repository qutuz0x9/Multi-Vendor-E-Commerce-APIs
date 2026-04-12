using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Infrastructure.Services;
using MultiVendorECommerce.Shared.Settings;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MultiVendorECommerce.API.Extensions;

public static class JwtServiceExtension
{
    public static IServiceCollection AddJwtService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<ITokenService, TokenService>();
        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<ECommerceDbContext>().AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // no grace period on expiry
            };
        });

        services.AddAuthorization();

        return services;
    }
}
