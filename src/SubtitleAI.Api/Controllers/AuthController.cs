using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubtitleAI.Application.Auth;

namespace SubtitleAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var r = await auth.RegisterAsync(request, ct);
        return r.Succeeded ? Ok(r.Response) : BadRequest(new { errors = r.Errors });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var r = await auth.LoginAsync(request, ct);
        return r.Succeeded ? Ok(r.Response) : Unauthorized(new { errors = r.Errors });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var r = await auth.RefreshAsync(request.RefreshToken, ct);
        return r.Succeeded ? Ok(r.Response) : Unauthorized(new { errors = r.Errors });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        id = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
        email = User.FindFirstValue("email") ?? User.FindFirstValue(ClaimTypes.Email),
        claims = User.Claims.Select(c => new { c.Type, c.Value })
    });
}