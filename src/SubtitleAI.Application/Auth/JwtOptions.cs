namespace SubtitleAI.Application.Auth;

public class JwtOptions
{
    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = "SubtitleAI";
    public string Audience { get; set; } = "SubtitleAI.Client";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}
