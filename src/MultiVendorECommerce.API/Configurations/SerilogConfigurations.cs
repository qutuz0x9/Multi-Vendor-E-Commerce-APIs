using Serilog;

namespace MultiVendorECommerce.API.Configurations
{
    public static class SerilogConfigurations
    {
        public static void ConfigureSerilog(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
