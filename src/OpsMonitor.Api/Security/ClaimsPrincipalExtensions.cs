using System.Security.Claims;

namespace OpsMonitor.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(value, out var id) ? id : 0;
    }

    public static string GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name) ?? "anonymous";
    }
}
