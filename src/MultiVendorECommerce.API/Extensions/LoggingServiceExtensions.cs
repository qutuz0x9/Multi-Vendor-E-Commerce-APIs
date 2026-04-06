using MultiVendorECommerce.API.Logging;

namespace MultiVendorECommerce.API.Extensions;

public static class LoggingServiceExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        // Register logging services here (e.g., Serilog, NLog, etc.)
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        return services;
    }
}
