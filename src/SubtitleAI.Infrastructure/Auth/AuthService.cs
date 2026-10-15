using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    IOptions<JwtOptions> options,
    AdminOptions adminOptions,
    EmailOptions emailOptions,
    IEmailSender emailSender,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwt = options.Value;

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return AuthResult.Fail(result.Errors.Select(ToAuthError).Distinct().ToArray());

        return AuthResult.Ok(await IssueAsync(user, ct));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
            return AuthResult.Fail("invalidEmailOrPassword");
        return AuthResult.Ok(await IssueAsync(user, ct));
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (stored is null || !stored.IsActive)
            return AuthResult.Fail("sessionExpired");

        var user = await users.FindByIdAsync(stored.UserId);
        if (user is null)
            return AuthResult.Fail("sessionExpired");

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

    public async Task<AuthOperationResult> RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken ct)
    {
        if (!emailSender.IsConfigured) return AuthOperationResult.Fail("emailDeliveryUnavailable");

        var user = await users.FindByEmailAsync(request.Email);
        if (user is null) return AuthOperationResult.Ok();

        try
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            await SendEmailAsync(
                user.Email!,
                "Reset your sublyAI password",
                "Set a new password",
                "We received a request to reset your password. If this was you, use the secure link below.",
                "/auth/reset-password",
                user.Id,
                token,
                ct);
            return AuthOperationResult.Ok();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send password reset email");
            return AuthOperationResult.Fail("emailDeliveryUnavailable");
        }
    }

    public async Task<AuthOperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(request.UserId);
        if (user is null) return AuthOperationResult.Fail("invalidOrExpiredPasswordReset");

        var result = await users.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded)
            return AuthOperationResult.Fail(result.Errors.Select(ToPasswordResetError).Distinct().ToArray());

        var activeTokens = await db.RefreshTokens.Where(token => token.UserId == user.Id && token.RevokedAt == null).ToListAsync(ct);
        foreach (var token in activeTokens) token.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return AuthOperationResult.Ok();
    }

    private async Task<AuthResponse> IssueAsync(ApplicationUser user, CancellationToken ct)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim("sub", user.Id),
                new Claim("email", user.Email!),
                ..(adminOptions.IsAdmin(user.Email) ? new[] { new Claim("role", "admin") } : [])
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
            RefreshToken = rawRefresh,
            AccessTokenExpiresAt = expires
        };
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string ToAuthError(IdentityError error) => error.Code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "emailAlreadyRegistered",
        "InvalidEmail" => "invalidEmail",
        "PasswordTooShort" => "passwordTooShort",
        "PasswordRequiresDigit" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
        "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars" => "passwordNotStrongEnough",
        _ => "registrationCouldNotBeCompleted"
    };

    private static string ToPasswordResetError(IdentityError error) => error.Code switch
    {
        "PasswordTooShort" => "passwordTooShort",
        "PasswordRequiresDigit" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
        "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars" => "passwordNotStrongEnough",
        _ => "invalidOrExpiredPasswordReset"
    };

    private async Task SendEmailAsync(
        string recipient,
        string subject,
        string title,
        string text,
        string path,
        string userId,
        string token,
        CancellationToken ct)
    {
        var url = $"{emailOptions.PublicBaseUrl.TrimEnd('/')}{path}?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
        var encodedUrl = HtmlEncoder.Default.Encode(url);
        var body = $"<h1>{HtmlEncoder.Default.Encode(title)}</h1><p>{HtmlEncoder.Default.Encode(text)}</p><p><a href=\"{encodedUrl}\">{HtmlEncoder.Default.Encode(title)}</a></p><p>This link expires automatically for your security.</p>";
        await emailSender.SendAsync(recipient, subject, body, ct);
    }
}
