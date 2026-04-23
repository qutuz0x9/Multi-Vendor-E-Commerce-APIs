using Microsoft.OpenApi;

namespace MultiVendorECommerce.API.Extensions;

public static class SwaggerServiceExtension
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Multi Vendor E-Commerce API",
                Description = "API for managing products, vendors, and offers in a multi-vendor e-commerce platform.",
                Contact = new OpenApiContact
                {
                    Name = "quzuz0x9",
                    Email = "quzuz0x9@example.com"
                }
            });
        });

        return services;
    }
}
