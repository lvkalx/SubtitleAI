using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SubtitleAI.Application.Summarization;

namespace SubtitleAI.UnitTests.Summarization;

public class MapReduceSummaryServiceTests
{
    private sealed class FakeLlm : ILlmClient
    {
        public string ModelName => "fake-model";
        public List<string> UserPrompts { get; } = [];
        public Func<string, string> Reply { get; set; } = _ => "short summary";

        public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct)
        {
            UserPrompts.Add(userPrompt);
            return Task.FromResult(Reply(userPrompt));
        }
    }

    private static MapReduceSummaryService Create(FakeLlm llm, int chunkSize = 100, int maxDepth = 5) =>
        new(llm,
            Options.Create(new SummaryOptions { ChunkSizeChars = chunkSize, MaxReduceDepth = maxDepth }),
            NullLogger<MapReduceSummaryService>.Instance);

    private static string LongText(int sentences) =>
        string.Join(' ', Enumerable.Range(1, sentences).Select(i => $"This is sentence number {i}."));

    [Fact]
    public async Task Blank_transcript_returns_null_without_calling_llm()
    {
        var llm = new FakeLlm();

        var result = await Create(llm).SummarizeAsync("  ", "en", CancellationToken.None);

        result.Should().BeNull();
        llm.UserPrompts.Should().BeEmpty();
    }

    [Fact]
    public async Task Short_text_needs_a_single_call()
    {
        var llm = new FakeLlm { Reply = _ => "  the summary  " };

        var result = await Create(llm, chunkSize: 1000).SummarizeAsync("Short text.", "en", CancellationToken.None);

        llm.UserPrompts.Should().ContainSingle();
        result.Should().NotBeNull();
        result!.Text.Should().Be("the summary");
        result.Model.Should().Be("fake-model");
    }

    [Fact]
    public async Task Long_text_is_mapped_per_chunk_and_then_reduced_once()
    {
        var text = LongText(30);
        var expectedChunks = TextChunker.Split(text, 100).Count;
        expectedChunks.Should().BeGreaterThan(1);
        var llm = new FakeLlm { Reply = _ => "x" }; // Tiny intermediate summaries require one reduce pass.

        var result = await Create(llm).SummarizeAsync(text, "en", CancellationToken.None);

        llm.UserPrompts.Should().HaveCount(expectedChunks + 1);
        llm.UserPrompts.Take(expectedChunks).Should().OnlyContain(p => p.Contains("<transcript>"));
        llm.UserPrompts.Last().Should().Contain("<summaries>");
        result!.Text.Should().Be("x");
    }

    [Fact]
    public async Task Reduce_is_repeated_while_partial_summaries_do_not_fit()
    {
        var text = LongText(60);
        // Each response is shorter than a source chunk (15 < 100), but 20 intermediate summaries
        // do not fit in one request. A second pass reduces them to four summaries that do fit.
        var llm = new FakeLlm { Reply = _ => "partial summary" };

        var result = await Create(llm, chunkSize: 100, maxDepth: 10).SummarizeAsync(text, "en", CancellationToken.None);

        result.Should().NotBeNull();
        llm.UserPrompts.Count(p => p.Contains("<summaries>")).Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task Throws_when_reduce_does_not_converge()
    {
        var llm = new FakeLlm { Reply = _ => string.Join(' ', Enumerable.Repeat("word", 40)) };

        var act = () => Create(llm, chunkSize: 100, maxDepth: 3).SummarizeAsync(LongText(60), "en", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Empty_llm_answer_is_an_error_not_an_empty_summary()
    {
        var llm = new FakeLlm { Reply = _ => "   " };

        var act = () => Create(llm, chunkSize: 1000).SummarizeAsync("Some text.", "en", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Disabled_service_is_unavailable_and_returns_null()
    {
        var svc = new DisabledSummaryService();

        svc.IsAvailable.Should().BeFalse();
        (await svc.SummarizeAsync("text", "en", CancellationToken.None)).Should().BeNull();
    }
}
