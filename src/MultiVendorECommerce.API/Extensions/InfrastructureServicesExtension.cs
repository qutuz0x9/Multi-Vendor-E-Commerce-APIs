using Microsoft.EntityFrameworkCore;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Infrastructure.Contexts;
using MultiVendorECommerce.Infrastructure.Services;
using MultiVendorECommerce.Infrastructure.UnitOfWork;
namespace MultiVendorECommerce.API.Extensions;

public static class InfrastructureServicesExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add infrastructure services here (e.g., database context, repositories, etc.)

        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ECommerceDbContext).Assembly.FullName);
                    npgsqlOptions.MapEnum<UserStatus>("user_status");
                    npgsqlOptions.MapEnum<VendorStatus>("vendor_status");
                    npgsqlOptions.MapEnum<BrandStatus>("brand_status");
                    npgsqlOptions.MapEnum<ProductStatus>("product_status");
                    npgsqlOptions.MapEnum<CategoryStatus>("Category_status");
                    npgsqlOptions.MapEnum<VendorOfferStatus>("vendor_offer_status");
                    npgsqlOptions.MapEnum<InventoryStatus>("inventory_status");
                    npgsqlOptions.MapEnum<OrderStatus>("order_status");
                    npgsqlOptions.MapEnum<PaymentStatus>("payment_status");
                    npgsqlOptions.MapEnum<CustomerAddressType>("customer_address_type");
                    npgsqlOptions.MapEnum<VendorAddressType>("vendor_address_type");
                }));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<IVendorOfferService, VendorOfferService>();
        services.AddScoped<ICartItemService, CartItemService>();
        services.AddScoped<ICustomerAddressService, CustomerAddressService>();

        return services;
    }
}
