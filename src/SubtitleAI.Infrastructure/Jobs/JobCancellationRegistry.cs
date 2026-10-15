using System.Collections.Concurrent;
using SubtitleAI.Application.Transcription;

namespace SubtitleAI.Infrastructure.Jobs;

public sealed class JobCancellationRegistry : IJobCancellationRegistry
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _sources = new();

    public IDisposable Register(Guid jobId, CancellationTokenSource source)
    {
        _sources[jobId] = source;
        return new Registration(() => ((ICollection<KeyValuePair<Guid, CancellationTokenSource>>)_sources)
            .Remove(new KeyValuePair<Guid, CancellationTokenSource>(jobId, source)));
    }

    public bool Cancel(Guid jobId)
    {
        if (!_sources.TryGetValue(jobId, out var source)) return false;
        source.Cancel();
        return true;
    }

    private sealed class Registration(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
