using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SubtitleAI.Application.Auth;

namespace SubtitleAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth, IWebHostEnvironment environment) : ControllerBase
{
    private const string RefreshCookieName = "subtitleai.refresh";

    [HttpPost("register"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var r = await auth.RegisterAsync(request, ct);
        return CompleteAuthentication(r, BadRequest);
    }

    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var r = await auth.LoginAsync(request, ct);
        return CompleteAuthentication(r, Unauthorized);
    }

    [HttpPost("refresh"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (string.IsNullOrEmpty(refreshToken)) return Unauthorized(new { errors = new[] { "Refresh session is missing." } });

        var r = await auth.RefreshAsync(refreshToken, ct);
        return CompleteAuthentication(r, Unauthorized);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrEmpty(refreshToken)) await auth.LogoutAsync(refreshToken, ct);
        Response.Cookies.Delete(RefreshCookieName, CookieOptions());
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

    private IActionResult CompleteAuthentication(AuthResult result, Func<object, IActionResult> error)
    {
        if (!result.Succeeded) return error(new { errors = result.Errors });

        // The raw refresh token never becomes part of JSON or browser storage.
        // It is intentionally kept in an HttpOnly, same-site cookie.
        var refreshToken = result.Response!.RefreshToken;
        Response.Cookies.Append(RefreshCookieName, refreshToken, CookieOptions());
        return Ok(result.Response);
    }

    private CookieOptions CookieOptions() => new()
    {
        HttpOnly = true,
        Secure = !environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = "/api/auth",
        MaxAge = TimeSpan.FromDays(14),
        IsEssential = true
    };
}
