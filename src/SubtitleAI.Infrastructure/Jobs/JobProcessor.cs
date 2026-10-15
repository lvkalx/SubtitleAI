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

public class JobProcessor(
    IServiceScopeFactory scopes,
    IJobQueue queue,
    IJobCancellationRegistry cancellations,
    IOptions<JobProcessingOptions> options,
    ILogger<JobProcessor> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        await RecoverAsync(stop);
        var workerCount = Math.Clamp(options.Value.MaxConcurrentJobs, 1, 2);
        log.LogInformation("Starting {WorkerCount} transcription workers", workerCount);
        await Task.WhenAll(Enumerable.Range(0, workerCount).Select(_ => RunWorkerAsync(stop)));
    }

    private async Task RunWorkerAsync(CancellationToken stop)
    {
        while (!stop.IsCancellationRequested)
        {
            QueuedJob queued;
            try { queued = await queue.DequeueAsync(stop); }
            catch (OperationCanceledException) { break; }

            try { await ProcessAsync(queued.Id, stop); }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { break; }
            catch (Exception ex) { log.LogError(ex, "Unexpected error while processing job {JobId}", queued.Id); }
            finally { queue.Complete(queued.UserId); }
        }
    }

    private async Task RecoverAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.MediaJobs.Where(j => j.Status == JobStatus.Processing)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JobStatus.Pending), ct);

            var pending = await db.MediaJobs.Where(j => j.Status == JobStatus.Pending)
                .OrderBy(j => j.CreatedAt).Select(j => new QueuedJob(j.Id, j.UserId)).ToListAsync(ct);
            foreach (var job in pending) queue.Enqueue(job.Id, job.UserId);
            log.LogInformation("Recovered {Count} pending jobs", pending.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { log.LogError(ex, "Failed to recover pending jobs"); }
    }

    private async Task<SummaryResult?> TrySummarizeAsync(
        ISummaryService summaries, Guid id, string text, string? language, TimeSpan timeout, CancellationToken ct)
    {
        if (!summaries.IsAvailable) return null;
        try
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutSource.CancelAfter(timeout);
            return await summaries.SummarizeAsync(text, language, timeoutSource.Token);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Summary failed for job {JobId}; subtitles are saved without a summary", id);
            return null;
        }
    }

    private async Task ProcessAsync(Guid id, CancellationToken stop)
    {
        using var scope = scopes.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();
        var started = await db.MediaJobs.Where(j => j.Id == id && j.Status == JobStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JobStatus.Processing), stop);
        if (started == 0) return;

        var job = await db.MediaJobs.AsNoTracking().FirstAsync(j => j.Id == id, stop);
        using var jobCancellation = CancellationTokenSource.CreateLinkedTokenSource(stop);
        using var registration = cancellations.Register(id, jobCancellation);
        if (!await db.MediaJobs.AnyAsync(j => j.Id == id && j.Status == JobStatus.Processing, jobCancellation.Token)) return;

        string? tmp = null;
        try
        {
            var ct = jobCancellation.Token;
            log.LogInformation("Processing job {JobId} ({File})", id, job.OriginalFileName);
            tmp = Path.Combine(Path.GetTempPath(), "subtitleai", id.ToString());
            Directory.CreateDirectory(tmp);

            var storage = sp.GetRequiredService<IFileStorage>();
            var input = Path.Combine(tmp, "input" + Path.GetExtension(job.BlobPath));
            await using (var src = await storage.OpenReadAsync(job.BlobPath, ct))
            await using (var dst = File.Create(input)) await src.CopyToAsync(dst, ct);

            var wav = Path.Combine(tmp, "audio.wav");
            await sp.GetRequiredService<IAudioExtractor>().ExtractWavAsync(input, wav, ct);
            var result = await sp.GetRequiredService<ITranscriptionService>().TranscribeAsync(wav, ct);
            if (result.Segments.Count == 0) throw new InvalidOperationException("No speech detected in the file.");

            var fullText = string.Join(" ", result.Segments.Select(s => s.Text));
            var summaryTimeout = TimeSpan.FromSeconds(Math.Max(1, sp.GetRequiredService<IOptions<SummaryOptions>>().Value.TimeoutSeconds));
            var summary = await TrySummarizeAsync(sp.GetRequiredService<ISummaryService>(), id, fullText, result.Language, summaryTimeout, ct);
            ct.ThrowIfCancellationRequested();

            db.Transcripts.Add(new Transcript { JobId = id, FullText = fullText, SrtContent = SubtitleFormatter.ToSrt(result.Segments), VttContent = SubtitleFormatter.ToVtt(result.Segments) });
            if (summary is not null) db.Summaries.Add(new Summary { JobId = id, Text = summary.Text, Model = summary.Model });
            await db.SaveChangesAsync(ct);
            ct.ThrowIfCancellationRequested();

            var completed = await db.MediaJobs.Where(j => j.Id == id && j.Status == JobStatus.Processing)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(j => j.DurationSeconds, Math.Max(0, (new FileInfo(wav).Length - 44) / 32000.0))
                    .SetProperty(j => j.Language, result.Language)
                    .SetProperty(j => j.Status, JobStatus.Done)
                    .SetProperty(j => j.CompletedAt, DateTime.UtcNow), ct);
            if (completed > 0) log.LogInformation("Job {JobId} done: {Segments} segments", id, result.Segments.Count);
        }
        catch (OperationCanceledException) when (jobCancellation.IsCancellationRequested && !stop.IsCancellationRequested)
        {
            log.LogInformation("Job {JobId} was cancelled", id);
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            log.LogError(ex, "Job {JobId} failed", id);
            var message = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            await db.MediaJobs.Where(j => j.Id == id && j.Status == JobStatus.Processing)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JobStatus.Failed)
                    .SetProperty(j => j.ErrorMessage, message).SetProperty(j => j.CompletedAt, DateTime.UtcNow), CancellationToken.None);
        }
        finally
        {
            if (tmp is not null) try { Directory.Delete(tmp, recursive: true); } catch { }
        }
    }
}
