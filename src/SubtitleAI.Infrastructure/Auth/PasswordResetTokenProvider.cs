using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SubtitleAI.Infrastructure.Persistence;

namespace SubtitleAI.Infrastructure.Auth;

/// <summary>Issues short-lived, user- and security-stamp-bound tokens for Identity operations.</summary>
public sealed class PasswordResetTokenProvider(IDataProtectionProvider dataProtection)
    : IUserTwoFactorTokenProvider<ApplicationUser>
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(3);
    private readonly ITimeLimitedDataProtector _protector = dataProtection
        .CreateProtector(nameof(PasswordResetTokenProvider), "v1")
        .ToTimeLimitedDataProtector();

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user) =>
        Task.FromResult(false);

    public async Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        var securityStamp = await manager.GetSecurityStampAsync(user);
        var payload = string.Join("\n", purpose, user.Id, securityStamp ?? string.Empty);
        var protectedPayload = _protector.Protect(Encoding.UTF8.GetBytes(payload), DateTimeOffset.UtcNow.Add(Lifetime));
        return WebEncoders.Base64UrlEncode(protectedPayload);
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        try
        {
            var protectedPayload = WebEncoders.Base64UrlDecode(token);
            var payload = Encoding.UTF8.GetString(_protector.Unprotect(protectedPayload, out var expires));
            if (expires < DateTimeOffset.UtcNow) return false;

            var values = payload.Split('\n');
            if (values.Length != 3 || values[0] != purpose || values[1] != user.Id) return false;

            return values[2] == (await manager.GetSecurityStampAsync(user) ?? string.Empty);
        }
        catch (Exception exception) when (exception is CryptographicException or FormatException)
        {
            return false;
        }
    }
}
