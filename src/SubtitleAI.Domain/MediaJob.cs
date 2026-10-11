namespace SubtitleAI.Domain;

public class MediaJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string BlobPath { get; set; } = default!;
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public double? DurationSeconds { get; set; }
    public string? Language { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public Transcript? Transcript { get; set; }
    public Summary? Summary { get; set; }
}