using Serilog;
using Serilog.Context;
namespace MultiVendorECommerce.API.Logging;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly Serilog.ILogger _logger;

    public AppLogger()
    {
        _logger = Log.ForContext<T>();
    }
    public void LogInformation(string message, params object[] args)
        => _logger.Information(message, args);

    public void LogWarning(string message, params object[] args)
        => _logger.Warning(message, args);

    public void LogError(Exception ex, string message, params object[] args)
        => _logger.Error(ex, message, args);

    public void LogDebug(string message, params object[] args)
        => _logger.Debug(message, args);

    public IDisposable BeginScope(string propertyName, object value)
        => LogContext.PushProperty(propertyName, value);
}
