using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpsMonitor.Api.Localization;

namespace OpsMonitor.Api.Controllers;

public static class ControllerApiErrorExtensions
{
    public static ObjectResult ApiError(this ControllerBase controller, int statusCode, string code, params object[] args)
    {
        var factory = controller.HttpContext.RequestServices.GetRequiredService<IApiErrorFactory>();
        var payload = factory.Create(controller.HttpContext, code, args);
        return new ObjectResult(payload) { StatusCode = statusCode };
    }
}
