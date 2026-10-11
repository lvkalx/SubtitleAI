using Microsoft.AspNetCore.Identity;

namespace SubtitleAI.Infrastructure.Persistence;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}