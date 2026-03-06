namespace OpsMonitor.Api.Localization;

public static class LocaleResolver
{
    public const string ZhCn = "zh-CN";
    public const string EnUs = "en-US";

    public static string ResolveFromHttpContext(HttpContext context)
    {
        var header = context.Request.Headers.AcceptLanguage.ToString();
        return Resolve(header);
    }

    public static string Resolve(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ZhCn;
        }

        var first = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(first))
        {
            return ZhCn;
        }

        if (first.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            return EnUs;
        }

        return ZhCn;
    }
}
