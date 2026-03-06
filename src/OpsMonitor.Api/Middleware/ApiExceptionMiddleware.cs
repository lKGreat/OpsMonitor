using System.Text.Json;
using OpsMonitor.Api.Localization;

namespace OpsMonitor.Api.Middleware;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;
    private readonly IApiErrorFactory _errorFactory;

    public ApiExceptionMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionMiddleware> logger,
        IApiErrorFactory errorFactory)
    {
        _next = next;
        _logger = logger;
        _errorFactory = errorFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            var payload = _errorFactory.Create(context, ex.Code, ex.Args);
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Unhandled argument exception.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            var payload = _errorFactory.Create(context, ErrorCodes.Common.InvalidRequest);
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var payload = _errorFactory.Create(context, ErrorCodes.Common.InternalError);
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
