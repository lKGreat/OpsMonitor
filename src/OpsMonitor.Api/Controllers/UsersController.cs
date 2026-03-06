using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Localization;
using OpsMonitor.Api.Services;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserInfoDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _userService.GetAllAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var id = await _userService.CreateAsync(request, ct);
        return Ok(new { id });
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult> Update(long id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var ok = await _userService.UpdateAsync(id, request, ct);
        return ok
            ? Ok()
            : this.ApiError(StatusCodes.Status404NotFound, ErrorCodes.User.NotFound);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult> Delete(long id, CancellationToken ct)
    {
        var ok = await _userService.DeleteAsync(id, ct);
        return ok
            ? Ok()
            : this.ApiError(StatusCodes.Status404NotFound, ErrorCodes.User.NotFound);
    }
}
