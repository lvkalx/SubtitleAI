namespace SubtitleAI.Infrastructure.Auth;

public sealed class EmailOptions
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string PublicBaseUrl { get; init; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(From) &&
        !string.IsNullOrWhiteSpace(PublicBaseUrl);
}
