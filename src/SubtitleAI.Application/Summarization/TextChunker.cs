using System.Text;

namespace SubtitleAI.Application.Summarization;

public static class TextChunker
{
    /// <summary>
    /// Splits text into chunks no longer than maxChars. Prefers sentence boundaries,
    /// then word boundaries, and falls back to a hard length cut when necessary.
    /// </summary>
    public static IReadOnlyList<string> Split(string text, int maxChars)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxChars, 2);

        text = text.Trim();
        if (text.Length == 0) return [];
        if (text.Length <= maxChars) return [text];

        var chunks = new List<string>();
        var current = new StringBuilder();

        void Flush()
        {
            if (current.Length == 0) return;
            chunks.Add(current.ToString());
            current.Clear();
        }

        foreach (var sentence in SplitSentences(text))
        {
            if (sentence.Length > maxChars)
            {
                Flush();
                chunks.AddRange(HardSplit(sentence, maxChars));
                continue;
            }

            if (current.Length > 0 && current.Length + 1 + sentence.Length > maxChars)
                Flush();

            if (current.Length > 0) current.Append(' ');
            current.Append(sentence);
        }

        Flush();
        return chunks;
    }

    private static IEnumerable<string> SplitSentences(string text)
    {
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            var latinEnd = c is '.' or '!' or '?' or '…';
            var cjkEnd = c is '。' or '！' or '？';
            if (!latinEnd && !cjkEnd) continue;

            // "3.14" or "e.g.x" is not a sentence boundary without whitespace or end of text.
            if (latinEnd && i + 1 < text.Length && !char.IsWhiteSpace(text[i + 1])) continue;

            var sentence = text[start..(i + 1)].Trim();
            if (sentence.Length > 0) yield return sentence;
            start = i + 1;
        }

        var tail = text[start..].Trim();
        if (tail.Length > 0) yield return tail;
    }

    private static IEnumerable<string> HardSplit(string s, int max)
    {
        var pos = 0;
        while (s.Length - pos > max)
        {
            var end = pos + max;
            var cut = s.LastIndexOf(' ', end - 1, max);
            if (cut <= pos)
            {
                cut = end;
                if (char.IsHighSurrogate(s[cut - 1])) cut--; // Do not split a surrogate pair.
            }

            var piece = s[pos..cut].Trim();
            if (piece.Length > 0) yield return piece;

            pos = cut;
            while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
        }

        if (pos < s.Length)
        {
            var rest = s[pos..].Trim();
            if (rest.Length > 0) yield return rest;
        }
    }
}
