using System.ComponentModel.DataAnnotations;

namespace SubtitleAI.Application.Auth;

public class RegisterRequest
{
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required, MinLength(8)] public string Password { get; set; } = default!;
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required] public string Password { get; set; } = default!;
}

public class RefreshRequest
{
    [Required] public string RefreshToken { get; set; } = default!;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime AccessTokenExpiresAt { get; set; }
}

public class AuthResult
{
    public bool Succeeded { get; init; }
    public AuthResponse? Response { get; init; }
    public string[] Errors { get; init; } = [];

    public static AuthResult Ok(AuthResponse r) => new() { Succeeded = true, Response = r };
    public static AuthResult Fail(params string[] errors) => new() { Errors = errors };
}

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct);
    Task LogoutAsync(string refreshToken, CancellationToken ct);
}