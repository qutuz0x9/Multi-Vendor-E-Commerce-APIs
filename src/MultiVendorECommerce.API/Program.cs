using MultiVendorECommerce.API.Configurations;
using MultiVendorECommerce.API.Extensions;
using Serilog;
using MultiVendorECommerce.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Load Environment Specific Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure Serilog
SerilogConfigurations.ConfigureSerilog(builder);


// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration); // Add infrastructure services (e.g., database context, repositories, etc.)
builder.Services.AddLoggingServices(); // Add logging services (e.g., Serilog, NLog, etc.)
builder.Services.AddControllers();
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Add Middlewares
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // Custom middleware for global exception handling
app.UseMiddleware<RequestLoggingMiddleware>(); // Custom middleware for logging HTTP requests and responses

// Serilog request logging with enriched context
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} : {StatusCode} ({Elapsed:0.0000}ms)";
    opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});


app.UseAuthorization();
app.MapControllers();
try
{
    Log.Information("Starting MultiVendor E-Commerce API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // ensures all buffered logs are flushed on shutdown
}
