namespace SubtitleAI.Application.Summarization;

public record SummaryResult(string Text, string Model);

/// <summary>
/// Делает саммари транскрипта. Это единственный интерфейс, который знает JobProcessor.
/// </summary>
public interface ISummaryService
{
    /// <summary>false, если LLM-провайдер не настроен (нет ключа): саммари тогда просто пропускается.</summary>
    bool IsAvailable { get; }

    /// <returns>null, если саммари недоступно или текст пустой.</returns>
    Task<SummaryResult?> SummarizeAsync(string transcript, string? language, CancellationToken ct);
}

/// <summary>
/// Единственное, что нужно реализовать под конкретного провайдера (Azure OpenAI и т.д.):
/// отправить два промпта и вернуть текст ответа. Разбиение на куски и map-reduce уже сделаны выше.
/// </summary>
public interface ILlmClient
{
    string ModelName { get; }
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct);
}

public class SummaryOptions
{
    /// <summary>Максимальный размер одного куска в символах (~4 символа на токен, запас под промпт и ответ).</summary>
    public int ChunkSizeChars { get; set; } = 12000;

    /// <summary>Защита от бесконечного reduce, если модель возвращает саммари не короче исходника.</summary>
    public int MaxReduceDepth { get; set; } = 5;

    /// <summary>Общий лимит времени на саммари одной задачи.</summary>
    public int TimeoutSeconds { get; set; } = 300;
}
