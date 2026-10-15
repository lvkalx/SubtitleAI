using SubtitleAI.Application.Transcription;

namespace SubtitleAI.Infrastructure.Jobs;

/// <summary>Keeps each user's recordings serial while allowing different users to run together.</summary>
public sealed class ChannelJobQueue : IJobQueue
{
    private readonly object _gate = new();
    private readonly List<QueuedJob> _pending = [];
    private readonly HashSet<string> _activeUsers = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _ready = new(0);

    public void Enqueue(Guid jobId, string userId)
    {
        lock (_gate) _pending.Add(new QueuedJob(jobId, userId));
        _ready.Release();
    }

    public async ValueTask<QueuedJob> DequeueAsync(CancellationToken ct)
    {
        while (true)
        {
            await _ready.WaitAsync(ct);
            lock (_gate)
            {
                var index = _pending.FindIndex(job => !_activeUsers.Contains(job.UserId));
                if (index < 0) continue;

                var job = _pending[index];
                _pending.RemoveAt(index);
                _activeUsers.Add(job.UserId);
                return job;
            }
        }
    }

    public void Complete(string userId)
    {
        var shouldSignal = false;
        lock (_gate)
        {
            _activeUsers.Remove(userId);
            shouldSignal = _pending.Any(job => !_activeUsers.Contains(job.UserId));
        }
        if (shouldSignal) _ready.Release();
    }
}
