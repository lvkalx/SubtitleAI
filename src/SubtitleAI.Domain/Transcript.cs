namespace SubtitleAI.Domain;

public class Transcript
{
    public Guid JobId { get; set; }
    public string FullText { get; set; } = default!;
}
