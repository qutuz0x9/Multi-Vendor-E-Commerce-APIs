using Serilog.Context;
using System.Diagnostics;

namespace MultiVendorECommerce.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var userAgent = context.Request.Headers.UserAgent.ToString();

        using var _ = LogContext.Push(
            new Serilog.Core.Enrichers.PropertyEnricher("RequestId", requestId),
            new Serilog.Core.Enrichers.PropertyEnricher("RequestPath", path.ToString()),
            new Serilog.Core.Enrichers.PropertyEnricher("HttpMethod", method)
            );

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            using var __ = LogContext.PushProperty("UserId", userId);
        }
        var sw = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Received HTTP {Method} {Path} with User-Agent: {UserAgent}", method, path, userAgent);
            await _next(context);

            sw.Stop();

            var statusCode = context.Response.StatusCode;
            var level = statusCode >= 500
                ? Microsoft.Extensions.Logging.LogLevel.Error
                : statusCode >= 400
                    ? Microsoft.Extensions.Logging.LogLevel.Warning
                    : Microsoft.Extensions.Logging.LogLevel.Information;

            _logger.Log(level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                method, path, statusCode, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "HTTP {Method} {Path} threw unhandled exception after {ElapsedMs}ms",
                method, path, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
