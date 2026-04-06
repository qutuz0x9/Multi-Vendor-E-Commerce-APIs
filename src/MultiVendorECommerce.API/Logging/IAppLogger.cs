namespace MultiVendorECommerce.API.Logging;

public interface IAppLogger<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
    void LogDebug(string message, params object[] args);
    /// <summary>Logs with additional structured properties pushed onto the log context.</summary>
    IDisposable BeginScope(string propertyName, object value);
}
