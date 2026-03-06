using OpsMonitor.Api.Contracts;

namespace OpsMonitor.Api.Localization;

public interface IApiErrorFactory
{
    ApiErrorDto Create(HttpContext context, string code, params object[] args);
    ApiErrorDto Create(string locale, string code, params object[] args);
}

public class ApiErrorFactory : IApiErrorFactory
{
    private readonly ITextLocalizer _localizer;

    public ApiErrorFactory(ITextLocalizer localizer)
    {
        _localizer = localizer;
    }

    public ApiErrorDto Create(HttpContext context, string code, params object[] args)
    {
        return Create(LocaleResolver.ResolveFromHttpContext(context), code, args);
    }

    public ApiErrorDto Create(string locale, string code, params object[] args)
    {
        return new ApiErrorDto(code, _localizer.Get(code, locale, args));
    }
}
