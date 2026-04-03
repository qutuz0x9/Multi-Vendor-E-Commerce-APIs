using Microsoft.EntityFrameworkCore;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Infrastructure.Contexts;

namespace MultiVendorECommerce.API.Configurations;

public static class InfrastructureConfigurations
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
                }));

        return services;
    }
}
