using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Localization;
using OpsMonitor.Api.Services;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/channels")]
[Authorize]
public class ChannelsController : ControllerBase
{
    private readonly IChannelService _channelService;

    public ChannelsController(IChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChannelDto>>> Get(CancellationToken ct)
    {
        return Ok(await _channelService.GetAllAsync(ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> Create([FromBody] ChannelUpsertDto dto, CancellationToken ct)
    {
        var id = await _channelService.CreateAsync(dto, ct);
        return Ok(new { id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(long id, [FromBody] ChannelUpsertDto dto, CancellationToken ct)
    {
        var ok = await _channelService.UpdateAsync(id, dto, ct);
        return ok
            ? Ok()
            : this.ApiError(StatusCodes.Status404NotFound, ErrorCodes.Channel.NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(long id, CancellationToken ct)
    {
        var ok = await _channelService.DeleteAsync(id, ct);
        return ok
            ? Ok()
            : this.ApiError(StatusCodes.Status404NotFound, ErrorCodes.Channel.NotFound);
    }
}
