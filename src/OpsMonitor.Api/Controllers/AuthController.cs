using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Security;
using OpsMonitor.Api.Services;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        if (response is null)
        {
            return Unauthorized(new { message = "Invalid credentials or account locked." });
        }
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoDto>> Me(CancellationToken ct)
    {
        var me = await _authService.GetMeAsync(User.GetUserId(), ct);
        if (me is null)
        {
            return Unauthorized();
        }
        return Ok(me);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var ok = await _authService.ChangePasswordAsync(User.GetUserId(), request, ct);
        if (!ok)
        {
            return BadRequest(new { message = "Current password is incorrect." });
        }
        return Ok(new { message = "Password changed." });
    }
}
