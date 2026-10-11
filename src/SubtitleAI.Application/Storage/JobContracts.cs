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

public interface IJobService
{
    Task<JobDto> CreateAsync(string userId, string fileName, string contentType, Stream content, CancellationToken ct);
    Task<IReadOnlyList<JobDto>> ListAsync(string userId, CancellationToken ct);
    Task<JobDto?> GetAsync(string userId, Guid id, CancellationToken ct);
    Task<Transcript?> GetTranscriptAsync(string userId, Guid id, CancellationToken ct);
    Task<Summary?> GetSummaryAsync(string userId, Guid id, CancellationToken ct);
}