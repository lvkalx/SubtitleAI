using AwesomeAssertions;
using SubtitleAI.Application.Summarization;

namespace SubtitleAI.UnitTests.Summarization;

public class TextChunkerTests
{
    private static string Normalize(string s) => string.Join(' ', s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    [Fact]
    public void Empty_text_gives_no_chunks() =>
        TextChunker.Split("   ", 100).Should().BeEmpty();

    [Fact]
    public void Short_text_is_one_chunk() =>
        TextChunker.Split("Hello world.", 100).Should().Equal("Hello world.");

    [Fact]
    public void Splits_on_sentence_boundaries()
    {
        var text = "First sentence here. Second sentence here. Third sentence here.";

        var chunks = TextChunker.Split(text, 45);

        chunks.Should().Equal("First sentence here. Second sentence here.", "Third sentence here.");
    }

    [Fact]
    public void Every_chunk_fits_and_no_words_are_lost()
    {
        var text = string.Join(' ', Enumerable.Range(1, 400).Select(i => $"Sentence number {i} is here."));

        var chunks = TextChunker.Split(text, 200);

        chunks.Should().OnlyContain(c => c.Length <= 200);
        Normalize(string.Join(' ', chunks)).Should().Be(Normalize(text));
    }

    [Fact]
    public void Text_without_punctuation_is_split_on_spaces()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 100)); // 499 characters and no punctuation.

        var chunks = TextChunker.Split(text, 50);

        chunks.Should().OnlyContain(c => c.Length <= 50);
        chunks.Should().OnlyContain(c => c.Split(new[] { ' ' }).All(w => w == "word")); // Words are not split.
        Normalize(string.Join(' ', chunks)).Should().Be(text);
    }

    [Fact]
    public void Text_without_any_spaces_is_cut_by_length()
    {
        var text = new string('a', 250);

        var chunks = TextChunker.Split(text, 100);

        chunks.Select(c => c.Length).Should().Equal(100, 100, 50);
    }

    [Fact]
    public void Decimal_numbers_are_not_sentence_ends()
    {
        var text = "Pi is about 3.14159 and e is 2.71828. Next sentence is longer than that.";

        var chunks = TextChunker.Split(text, 40);

        chunks[0].Should().Be("Pi is about 3.14159 and e is 2.71828.");
    }
}
