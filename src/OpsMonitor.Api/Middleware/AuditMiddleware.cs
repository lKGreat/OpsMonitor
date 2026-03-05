using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Security;
using SqlSugar;

namespace OpsMonitor.Api.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ISqlSugarClient db)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;
        var shouldAudit = path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) &&
                          method is "POST" or "PUT" or "DELETE";

        Exception? exception = null;
        if (!shouldAudit)
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            var user = context.User?.Identity?.IsAuthenticated == true
                ? context.User.GetUserName()
                : null;
            await db.Insertable(new SysAuditLog
            {
                CreatedAt = DateTime.UtcNow,
                UserName = user,
                Method = method,
                Path = path,
                Ip = context.Connection.RemoteIpAddress?.ToString(),
                Status = exception is null ? "OK" : "ERR",
                Message = exception?.Message
            }).ExecuteCommandAsync();
        }
    }
}
