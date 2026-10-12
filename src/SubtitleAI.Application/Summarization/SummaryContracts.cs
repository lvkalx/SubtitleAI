namespace SubtitleAI.Application.Summarization;

public record SummaryResult(string Text, string Model);

/// <summary>
/// Summarizes a transcript. This is the only summarization interface known to JobProcessor.
/// </summary>
public interface ISummaryService
{
    /// <summary>False when no LLM provider is configured; the summary step is then skipped.</summary>
    bool IsAvailable { get; }

    /// <returns>Null when a summary is unavailable or the text is empty.</returns>
    Task<SummaryResult?> SummarizeAsync(string transcript, string? language, CancellationToken ct);
}

/// <summary>
/// The only provider-specific implementation: submit prompts and return the response text.
/// Chunking and map-reduce orchestration are handled above.
/// </summary>
public interface ILlmClient
{
    string ModelName { get; }
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct);
}

public class SummaryOptions
{
    /// <summary>Maximum chunk size in characters (roughly four characters per token).</summary>
    public int ChunkSizeChars { get; set; } = 12000;

    /// <summary>Prevents infinite reduction when the model does not shorten the text.</summary>
    public int MaxReduceDepth { get; set; } = 5;

    /// <summary>Total time limit for one job's summary.</summary>
    public int TimeoutSeconds { get; set; } = 300;
}
