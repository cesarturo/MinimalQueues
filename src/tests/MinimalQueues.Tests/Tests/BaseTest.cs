using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Tests.Internal;

namespace Tests;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public abstract class BaseTest
{
    private readonly IMessageSender _messageSender;
    private readonly Func<IHost> _createReceiverHostDelegate;
    

    protected BaseTest(Func<IMessageSender> createMessageSenderDelegate, Func<IHost> createReceiverHostDelegate)
    {
        _messageSender = createMessageSenderDelegate();
        _createReceiverHostDelegate = createReceiverHostDelegate;
    }

    [SetUp]
    public void Setup()
    {
        try
        {
            Assume.That(_messageSender.PurgeQueueAsync, Is.True, "Failed to purge the queue.");
        }
        catch
        {//Disposing here because Teardown is only called if Setup() is successful (see NUnit docs)
            _messageSender.Dispose();
            throw;
        }
    }

    [TearDown]
    public void Teardown()
    {
        _messageSender.Dispose();
    }

    [Test]
    public async Task All_messages_are_processed()
    {
        using var receiverHost = await StartNewReceiverHost();

        SendMessages(count: 5, receiverExecutionTime: TimeSpan.FromSeconds(10));

        SendMessages(count: 100, receiverExecutionTime: TimeSpan.FromMilliseconds(200));

        await Wait(10);

        Assert.That(() => receiverHost.GetProcessedMessages(), Has.Count.EqualTo(_messageSender.SentMessages.Count).After(60).Seconds.PollEvery(3).Seconds);

        Assert.That(() => receiverHost.GetProcessedMessages(), Is.EquivalentTo(_messageSender.SentMessages).After(60).Seconds.PollEvery(3).Seconds);
    }

    [Test]
    public async Task Stops_processing_messages()
    {
        using var receiverHost = await StartNewReceiverHost();

        SendMessages(count: 20);

        await Wait(seconds: 5);

        await receiverHost.StopAsync();

        var expected = receiverHost.GetProcessedMessages().ToArray();

        SendMessages(count: 5);

        await Wait(seconds: 5);

        Assert.That(() => receiverHost.GetProcessedMessages(), Is.EquivalentTo(expected));
    }

    private void SendMessages(int count, TimeSpan? receiverExecutionTime = null)
    {
        Assume.That(
            async () => await _messageSender.SendMessagesAsync(count, receiverExecutionTime)
            , Throws.Nothing, "Error sending messages.");
    }

    private Task Wait(int seconds) => Task.Delay(TimeSpan.FromSeconds(seconds));

    private async Task<IHost> StartNewReceiverHost()
    {
        var receiverHost = _createReceiverHostDelegate();

        await StartHost(receiverHost);

        return receiverHost;


        static async Task StartHost(IHost host)
        {
            try
            {
                await host.StartAsync();
            }
            catch 
            {
                host.Dispose();

                throw;
            }
        }
    }
}