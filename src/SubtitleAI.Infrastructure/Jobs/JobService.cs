using Microsoft.EntityFrameworkCore;
using SubtitleAI.Application.Jobs;
using SubtitleAI.Application.Storage;
using SubtitleAI.Application.Transcription;
using SubtitleAI.Domain;
using SubtitleAI.Infrastructure.Persistence;

namespace SubtitleAI.Infrastructure.Jobs;

public class JobService(AppDbContext db, IFileStorage storage, IJobQueue queue) : IJobService
{
    public async Task<JobDto> CreateAsync(
        string userId, string fileName, string contentType, Stream content, CancellationToken ct)
    {
        var job = new MediaJob
        {
            UserId = userId,
            OriginalFileName = fileName,
            Status = JobStatus.Pending
        };
        job.BlobPath = $"{userId}/{job.Id}{Path.GetExtension(fileName).ToLowerInvariant()}";

        await storage.SaveAsync(job.BlobPath, content, contentType, ct);
        try
        {
            db.MediaJobs.Add(job);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(job.BlobPath, CancellationToken.None);
            throw;
        }

        queue.Enqueue(job.Id);
        return ToDto(job);
    }

    public async Task<IReadOnlyList<JobDto>> ListAsync(string userId, CancellationToken ct)
    {
        var jobs = await db.MediaJobs.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
        return jobs.Select(ToDto).ToList();
    }

    public async Task<JobDto?> GetAsync(string userId, Guid id, CancellationToken ct)
    {
        var job = await db.MediaJobs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        return job is null ? null : ToDto(job);
    }

    public async Task<Transcript?> GetTranscriptAsync(string userId, Guid id, CancellationToken ct) =>
        await db.Transcripts.AsNoTracking()
            .Where(t => t.JobId == id && db.MediaJobs.Any(j => j.Id == id && j.UserId == userId))
            .FirstOrDefaultAsync(ct);

    public async Task<Summary?> GetSummaryAsync(string userId, Guid id, CancellationToken ct) =>
        await db.Summaries.AsNoTracking()
            .Where(s => s.JobId == id && db.MediaJobs.Any(j => j.Id == id && j.UserId == userId))
            .FirstOrDefaultAsync(ct);

    private static JobDto ToDto(MediaJob j) => new(
        j.Id, j.OriginalFileName, j.Status.ToString(), j.DurationSeconds,
        j.Language, j.ErrorMessage, j.CreatedAt, j.CompletedAt);
}