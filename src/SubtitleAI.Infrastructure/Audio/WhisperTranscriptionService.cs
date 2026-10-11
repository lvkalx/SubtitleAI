using Microsoft.Extensions.Options;
using SubtitleAI.Application.Transcription;
using Whisper.net;

namespace SubtitleAI.Infrastructure.Transcription;

public class WhisperSettings
{
    public string ModelPath { get; set; } = default!;
}

public sealed class WhisperTranscriptionService : ITranscriptionService, IDisposable
{
    private readonly WhisperFactory _factory;

    public WhisperTranscriptionService(IOptions<WhisperSettings> options)
    {
        var path = options.Value.ModelPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            throw new FileNotFoundException($"Whisper model not found: '{path}'.");
        _factory = WhisperFactory.FromPath(path);
    }

    public async Task<TranscriptionResult> TranscribeAsync(string wavPath, CancellationToken ct)
    {
        await using var processor = _factory.CreateBuilder().WithLanguage("auto").Build();
        await using var stream = File.OpenRead(wavPath);

        var segments = new List<TranscriptSegment>();
        string? language = null;

        await foreach (var s in processor.ProcessAsync(stream, ct))
        {
            language ??= s.Language;
            if (!string.IsNullOrWhiteSpace(s.Text))
                segments.Add(new TranscriptSegment(s.Start, s.End, s.Text.Trim()));
        }
        return new TranscriptionResult(language, segments);
    }

    public void Dispose() => _factory.Dispose();
}