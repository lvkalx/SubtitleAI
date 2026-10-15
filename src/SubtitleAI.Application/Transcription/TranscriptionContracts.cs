namespace SubtitleAI.Application.Transcription;

public record TranscriptSegment(TimeSpan Start, TimeSpan End, string Text);

public record TranscriptionResult(string? Language, IReadOnlyList<TranscriptSegment> Segments);

public interface ITranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(string wavPath, CancellationToken ct);
}

public interface IAudioExtractor
{
    Task ExtractWavAsync(string inputPath, string outputWavPath, CancellationToken ct);
}

public interface IJobQueue
{
    void Enqueue(Guid jobId, string userId);
    ValueTask<QueuedJob> DequeueAsync(CancellationToken ct);
    void Complete(string userId);
}

public record QueuedJob(Guid Id, string UserId);

public interface IJobCancellationRegistry
{
    IDisposable Register(Guid jobId, CancellationTokenSource source);
    bool Cancel(Guid jobId);
}
