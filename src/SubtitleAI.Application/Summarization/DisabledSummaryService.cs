namespace SubtitleAI.Application.Summarization;

/// <summary>Used when no LLM provider is configured.</summary>
public sealed class DisabledSummaryService : ISummaryService
{
    public bool IsAvailable => false;

    public Task<SummaryResult?> SummarizeAsync(string transcript, string? language, CancellationToken ct) =>
        Task.FromResult<SummaryResult?>(null);
}
