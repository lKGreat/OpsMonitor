using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Api.Domain;
using SqlSugar;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ISqlSugarClient _db;

    public DashboardController(ISqlSugarClient db)
    {
        _db = db;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> Summary(CancellationToken ct)
    {
        var totalMonitors = await _db.Queryable<MonMonitor>().CountAsync();
        var enabledMonitors = await _db.Queryable<MonMonitor>().CountAsync(x => x.IsEnabled);
        var firingAlerts = await _db.Queryable<AlertEvent>().CountAsync(x => x.State == AlertState.Firing);
        var resolvedToday = await _db.Queryable<AlertEvent>()
            .CountAsync(x => x.State == AlertState.Resolved && x.ResolvedAt >= DateTime.UtcNow.Date);
        return Ok(new
        {
            totalMonitors,
            enabledMonitors,
            firingAlerts,
            resolvedToday
        });
    }
}
