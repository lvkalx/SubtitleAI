using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SubtitleAI.Application.Auth;
using SubtitleAI.Domain;
using SubtitleAI.Infrastructure.Persistence;

namespace SubtitleAI.Infrastructure.Auth;

public class AuthService(
    UserManager<ApplicationUser> users,
    AppDbContext db,
    IOptions<JwtOptions> options) : IAuthService
{
    private readonly JwtOptions _jwt = options.Value;

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return AuthResult.Fail(result.Errors.Select(e => e.Description).ToArray());

        return AuthResult.Ok(await IssueAsync(user, ct));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
            return AuthResult.Fail("Invalid email or password.");

        return AuthResult.Ok(await IssueAsync(user, ct));
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (stored is null || !stored.IsActive)
            return AuthResult.Fail("Invalid refresh token.");

        var user = await users.FindByIdAsync(stored.UserId);
        if (user is null)
            return AuthResult.Fail("Invalid refresh token.");

        stored.RevokedAt = DateTime.UtcNow;
        return AuthResult.Ok(await IssueAsync(user, ct));
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (stored is { RevokedAt: null })
        {
            stored.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task<AuthResponse> IssueAsync(ApplicationUser user, CancellationToken ct)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim("sub", user.Id),
                new Claim("email", user.Email!)
            ]),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                SecurityAlgorithms.HmacSha256)
        };
        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);

        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(rawRefresh),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
        });
        await db.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = expires
        };
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
