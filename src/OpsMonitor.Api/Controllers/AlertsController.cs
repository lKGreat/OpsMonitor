using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Localization;
using OpsMonitor.Api.Security;
using OpsMonitor.Api.Services;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertQueryService _alertQueryService;

    public AlertsController(IAlertQueryService alertQueryService)
    {
        _alertQueryService = alertQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AlertDto>>> Query([FromQuery] string? state, CancellationToken ct)
    {
        return Ok(await _alertQueryService.QueryAsync(state, ct));
    }

    [HttpPost("{id:long}/ack")]
    public async Task<ActionResult> Ack(long id, [FromBody] AckAlertRequest request, CancellationToken ct)
    {
        var ok = await _alertQueryService.AckAsync(id, User.GetUserName(), ct);
        return ok
            ? Ok()
            : this.ApiError(StatusCodes.Status404NotFound, ErrorCodes.Alert.NotFound);
    }
}
