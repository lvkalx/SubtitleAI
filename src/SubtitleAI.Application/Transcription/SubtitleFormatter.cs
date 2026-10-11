using System.Text;

namespace SubtitleAI.Application.Transcription;

public static class SubtitleFormatter
{
    public static string ToSrt(IEnumerable<TranscriptSegment> segments)
    {
        var sb = new StringBuilder();
        var index = 1;
        foreach (var s in segments)
        {
            sb.AppendLine(index.ToString());
            sb.AppendLine($"{Format(s.Start, ',')} --> {Format(s.End, ',')}");
            sb.AppendLine(s.Text.Trim());
            sb.AppendLine();
            index++;
        }
        return sb.ToString();
    }

    public static string ToVtt(IEnumerable<TranscriptSegment> segments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();
        foreach (var s in segments)
        {
            sb.AppendLine($"{Format(s.Start, '.')} --> {Format(s.End, '.')}");
            sb.AppendLine(s.Text.Trim());
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string Format(TimeSpan t, char separator) =>
        $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}{separator}{t.Milliseconds:000}";
}