namespace SubtitleAI.Application.Summarization;

/// <summary>Используется, когда LLM-провайдер не настроен.</summary>
public sealed class DisabledSummaryService : ISummaryService
{
    public bool IsAvailable => false;

    public Task<SummaryResult?> SummarizeAsync(string transcript, string? language, CancellationToken ct) =>
        Task.FromResult<SummaryResult?>(null);
}
