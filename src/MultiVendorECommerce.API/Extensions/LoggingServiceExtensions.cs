using MultiVendorECommerce.API.Logging;
using SharedLogging = MultiVendorECommerce.Shared.Logging;

namespace MultiVendorECommerce.API.Extensions;

public static class LoggingServiceExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        // Register logging services here (e.g., Serilog, NLog, etc.)
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        services.AddSingleton(typeof(SharedLogging.IAppLogger<>), typeof(AppLogger<>));
        return services;
    }
}
