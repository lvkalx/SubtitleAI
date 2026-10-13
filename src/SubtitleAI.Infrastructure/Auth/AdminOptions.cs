namespace SubtitleAI.Infrastructure.Auth;

public sealed class AdminOptions
{
    public string[] Emails { get; init; } = ["sasha.livak303@gmail.com"];

    public bool IsAdmin(string? email) =>
        !string.IsNullOrWhiteSpace(email) &&
        Emails.Any(value => string.Equals(value, email, StringComparison.OrdinalIgnoreCase));
}
