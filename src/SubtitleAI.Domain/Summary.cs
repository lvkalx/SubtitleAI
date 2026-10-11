namespace SubtitleAI.Domain;

public class Summary
{
    public Guid JobId { get; set; }
    public string Text { get; set; } = default!;
    public string Model { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}