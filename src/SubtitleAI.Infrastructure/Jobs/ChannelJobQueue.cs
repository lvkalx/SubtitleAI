using System.Threading.Channels;
using SubtitleAI.Application.Transcription;

namespace SubtitleAI.Infrastructure.Jobs;

public class ChannelJobQueue : IJobQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public void Enqueue(Guid jobId) => _channel.Writer.TryWrite(jobId);

    public ValueTask<Guid> DequeueAsync(CancellationToken ct) => _channel.Reader.ReadAsync(ct);
}