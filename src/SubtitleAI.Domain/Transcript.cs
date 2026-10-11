namespace SubtitleAI.Domain;

public class Transcript
{
    public Guid JobId { get; set; }
    public string FullText { get; set; } = default!;
    public string SrtContent { get; set; } = default!;
    public string VttContent { get; set; } = default!;
}