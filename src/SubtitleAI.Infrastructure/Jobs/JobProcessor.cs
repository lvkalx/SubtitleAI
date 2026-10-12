using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitleAI.Application.Storage;
using SubtitleAI.Application.Summarization;
using SubtitleAI.Application.Transcription;
using SubtitleAI.Domain;
using SubtitleAI.Infrastructure.Persistence;

namespace SubtitleAI.Infrastructure.Jobs;

public class JobProcessor(IServiceScopeFactory scopes, IJobQueue queue, ILogger<JobProcessor> log)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        await RecoverAsync(stop);

        while (!stop.IsCancellationRequested)
        {
            Guid id;
            try
            {
                id = await queue.DequeueAsync(stop);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await ProcessAsync(id, stop);
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unexpected error while processing job {JobId}", id);
            }
        }
    }

    // Return unfinished jobs to the queue after a restart.
    private async Task RecoverAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.MediaJobs
                .Where(j => j.Status == JobStatus.Processing)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JobStatus.Pending), ct);

            var pending = await db.MediaJobs
                .Where(j => j.Status == JobStatus.Pending)
                .OrderBy(j => j.CreatedAt)
                .Select(j => j.Id)
                .ToListAsync(ct);

            foreach (var id in pending) queue.Enqueue(id);
            log.LogInformation("Recovered {Count} pending jobs", pending.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Failed to recover pending jobs");
        }
    }

    private async Task<SummaryResult?> TrySummarizeAsync(
        ISummaryService summaries, Guid id, string text, string? language, TimeSpan timeout, CancellationToken ct)
    {
        if (!summaries.IsAvailable)
        {
            log.LogInformation("Summary skipped for job {JobId}: no LLM provider configured", id);
            return null;
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            return await summaries.SummarizeAsync(text, language, cts.Token);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // Application shutdown, not a summarization failure.
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Summary failed for job {JobId}; subtitles are saved without a summary", id);
            return null;
        }
    }

    private async Task ProcessAsync(Guid id, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();

        var job = await db.MediaJobs.FirstOrDefaultAsync(j => j.Id == id, ct);
        if (job is null || job.Status != JobStatus.Pending) return;

        job.Status = JobStatus.Processing;
        await db.SaveChangesAsync(ct);
        log.LogInformation("Processing job {JobId} ({File})", id, job.OriginalFileName);

        var tmp = Path.Combine(Path.GetTempPath(), "subtitleai", id.ToString());
        Directory.CreateDirectory(tmp);

        try
        {
            var storage = sp.GetRequiredService<IFileStorage>();
            var extractor = sp.GetRequiredService<IAudioExtractor>();
            var stt = sp.GetRequiredService<ITranscriptionService>();

            var input = Path.Combine(tmp, "input" + Path.GetExtension(job.BlobPath));
            await using (var src = await storage.OpenReadAsync(job.BlobPath, ct))
            await using (var dst = File.Create(input))
            {
                await src.CopyToAsync(dst, ct);
            }

            var wav = Path.Combine(tmp, "audio.wav");
            await extractor.ExtractWavAsync(input, wav, ct);

            var result = await stt.TranscribeAsync(wav, ct);
            if (result.Segments.Count == 0)
                throw new InvalidOperationException("No speech detected in the file.");

            var fullText = string.Join(" ", result.Segments.Select(s => s.Text));
            db.Transcripts.Add(new Transcript
            {
                JobId = id,
                FullText = fullText,
                SrtContent = SubtitleFormatter.ToSrt(result.Segments),
                VttContent = SubtitleFormatter.ToVtt(result.Segments)
            });

            // Summarization is optional: a failure must not prevent subtitle generation.
            var summaryTimeout = TimeSpan.FromSeconds(
                Math.Max(1, sp.GetRequiredService<IOptions<SummaryOptions>>().Value.TimeoutSeconds));
            var summary = await TrySummarizeAsync(
                sp.GetRequiredService<ISummaryService>(), id, fullText, result.Language, summaryTimeout, ct);
            if (summary is not null)
            {
                db.Summaries.Add(new Summary { JobId = id, Text = summary.Text, Model = summary.Model });
            }

            job.DurationSeconds = Math.Max(0, (new FileInfo(wav).Length - 44) / 32000.0);
            job.Language = result.Language;
            job.Status = JobStatus.Done;
            job.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);

            log.LogInformation("Job {JobId} done: {Segments} segments, language {Lang}",
                id, result.Segments.Count, result.Language);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // Application shutdown: the job will be re-queued on the next start.
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Job {JobId} failed", id);
            db.ChangeTracker.Clear();

            var message = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            await db.MediaJobs
                .Where(j => j.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(j => j.Status, JobStatus.Failed)
                    .SetProperty(j => j.ErrorMessage, message)
                    .SetProperty(j => j.CompletedAt, DateTime.UtcNow),
                    CancellationToken.None);
        }
        finally
        {
            try { Directory.Delete(tmp, recursive: true); } catch { /* Temporary files are non-critical. */ }
        }
    }
}
