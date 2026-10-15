using SubtitleAI.Domain;

namespace SubtitleAI.Application.Jobs;

public record JobDto(
    Guid Id,
    string FileName,
    string Status,
    double? DurationSeconds,
    string? Language,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record SummaryDto(string Text, string Model, DateTime CreatedAt);

/// <summary>Stable envelope used by list endpoints. Page numbers are one-based.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public interface IJobService
{
    Task<JobDto> CreateAsync(string userId, string fileName, string contentType, Stream content, CancellationToken ct);
    Task<PagedResult<JobDto>> ListAsync(string userId, int page, int pageSize, CancellationToken ct);
    Task<JobDto?> GetAsync(string userId, Guid id, CancellationToken ct);
    Task<bool> CancelAsync(string userId, Guid id, CancellationToken ct);
    Task<Transcript?> GetTranscriptAsync(string userId, Guid id, CancellationToken ct);
    Task<Summary?> GetSummaryAsync(string userId, Guid id, CancellationToken ct);
}
