using CliWrap;
using CliWrap.Buffered;
using SubtitleAI.Application.Transcription;

namespace SubtitleAI.Infrastructure.Audio;

public class FfmpegAudioExtractor : IAudioExtractor
{
    public async Task ExtractWavAsync(string inputPath, string outputWavPath, CancellationToken ct)
    {
        var result = await Cli.Wrap("ffmpeg")
            .WithArguments(["-y", "-i", inputPath, "-vn", "-ac", "1", "-ar", "16000", "-c:a", "pcm_s16le", outputWavPath])
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(ct);

        if (result.ExitCode != 0)
        {
            var err = result.StandardError;
            if (err.Length > 500) err = err[^500..];
            throw new InvalidOperationException($"ffmpeg failed (code {result.ExitCode}): {err}");
        }
    }
}