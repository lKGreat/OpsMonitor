using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Security;
using SqlSugar;

namespace OpsMonitor.Api.Middleware;

public class RequirePasswordChangeMiddleware
{
    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login",
        "/api/auth/logout",
        "/api/auth/me",
        "/api/auth/change-password"
    };

    private readonly RequestDelegate _next;

    public RequirePasswordChangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ISqlSugarClient db)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
            AllowedPaths.Contains(path) ||
            context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userId = context.User.GetUserId();
        if (userId <= 0)
        {
            await _next(context);
            return;
        }

        var user = await db.Queryable<SysUser>().InSingleAsync(userId);
        if (user is not null && user.RequirePasswordChange)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"message\":\"PASSWORD_CHANGE_REQUIRED\"}");
            return;
        }

        await _next(context);
    }
}
