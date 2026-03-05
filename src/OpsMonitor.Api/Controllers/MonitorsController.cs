using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Security;
using OpsMonitor.Api.Services;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/monitors")]
[Authorize]
public class MonitorsController : ControllerBase
{
    private readonly IMonitorService _monitorService;

    public MonitorsController(IMonitorService monitorService)
    {
        _monitorService = monitorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MonitorListItemDto>>> Get(CancellationToken ct)
    {
        return Ok(await _monitorService.GetListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] MonitorUpsertDto dto, CancellationToken ct)
    {
        var id = await _monitorService.CreateAsync(User.GetUserId(), dto, ct);
        return Ok(new { id });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<MonitorDetailDto>> GetById(long id, CancellationToken ct)
    {
        var row = await _monitorService.GetAsync(id, ct);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult> Update(long id, [FromBody] MonitorUpsertDto dto, CancellationToken ct)
    {
        var ok = await _monitorService.UpdateAsync(id, dto, ct);
        return ok ? Ok() : NotFound();
    }

    [HttpPost("{id:long}/enable")]
    public async Task<ActionResult> Enable(long id, CancellationToken ct)
    {
        return await _monitorService.SetEnabledAsync(id, true, ct) ? Ok() : NotFound();
    }

    [HttpPost("{id:long}/disable")]
    public async Task<ActionResult> Disable(long id, CancellationToken ct)
    {
        return await _monitorService.SetEnabledAsync(id, false, ct) ? Ok() : NotFound();
    }

    [HttpGet("{id:long}/results")]
    public async Task<ActionResult<List<CheckResultDto>>> Results(long id, [FromQuery] int days = 7, CancellationToken ct = default)
    {
        return Ok(await _monitorService.GetResultsAsync(id, days, ct));
    }
}
