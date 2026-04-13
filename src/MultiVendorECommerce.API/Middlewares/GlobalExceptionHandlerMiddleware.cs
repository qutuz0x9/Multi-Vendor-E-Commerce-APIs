using System.Net;
using System.Text.Json;
using FluentValidation;

namespace MultiVendorECommerce.API.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation Exception: {ExceptionType} — {Message}", ex.GetType().Name, ex.Message);

            await WriteValidationErrorResponse(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception: {ExceptionType} — {Message}", ex.GetType().Name, ex.Message);

            await WriteErrorResponse(context, ex);
        }
    }
    private static async Task WriteValidationErrorResponse(HttpContext context, ValidationException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = "One or more validation errors occurred.",
            errors,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }

    private static async Task WriteErrorResponse(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = context.Response.StatusCode == 500
                ? "An unexpected error occurred."
                : ex.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
