namespace SubtitleAI.Infrastructure.Jobs;

public sealed class JobProcessingOptions
{
    public int MaxConcurrentJobs { get; init; } = 2;
}
