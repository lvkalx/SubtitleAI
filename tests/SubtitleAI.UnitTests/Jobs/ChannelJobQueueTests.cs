using AwesomeAssertions;
using SubtitleAI.Infrastructure.Jobs;

namespace SubtitleAI.UnitTests.Jobs;

public class ChannelJobQueueTests
{
    [Fact]
    public async Task Allows_another_user_to_start_while_the_same_users_second_job_waits()
    {
        var queue = new ChannelJobQueue();
        var firstForAlice = Guid.NewGuid();
        var secondForAlice = Guid.NewGuid();
        var forBob = Guid.NewGuid();

        queue.Enqueue(firstForAlice, "alice");
        queue.Enqueue(secondForAlice, "alice");
        queue.Enqueue(forBob, "bob");

        var first = await queue.DequeueAsync(CancellationToken.None);
        var second = await queue.DequeueAsync(CancellationToken.None);

        first.Id.Should().Be(firstForAlice);
        second.Id.Should().Be(forBob);

        queue.Complete("alice");

        var nextForAlice = await queue.DequeueAsync(CancellationToken.None);
        nextForAlice.Id.Should().Be(secondForAlice);
    }
}
