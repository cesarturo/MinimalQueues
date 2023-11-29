using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;
using NUnit.Framework;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public abstract class BaseTest
{
    private readonly Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> _configureListener;
    private readonly IMessageSender _sender;
    private readonly IMessageReceiver _receiver;

    protected BaseTest(Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> configureListener
        , IMessageSender sender
        , IMessageReceiver receiver)
    {
        _configureListener = configureListener;

        _sender = sender;

        _receiver = receiver;
    }

    [SetUp]
    public async Task Setup()
    {
        try
        {
            await _sender.PurgeQueueAsync();

            await _receiver.StartAsync(_configureListener);
        }
        catch
        {//Disposing here because Teardown is only called if Setup() is successful (see NUnit docs)
            _sender?.Dispose();
            _receiver?.Dispose();
        }
    }

    [TearDown]
    public void Teardown()
    {
        _sender.Dispose();

        _receiver.Dispose();
    }

    [Test]
    public async Task All_messages_were_processed()
    {
        await _sender.SendMessagesAsync(5, TimeSpan.FromSeconds(10));

        await _sender.SendMessagesAsync(100, TimeSpan.FromMilliseconds(200));

        await Task.Delay(TimeSpan.FromSeconds(10));

        Assert.That(() => _receiver.ProcessedMessages, Is.EquivalentTo(_sender.SentMessages).After(50).Seconds.PollEvery(3).Seconds);
    }
}