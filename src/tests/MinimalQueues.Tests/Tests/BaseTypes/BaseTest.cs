using Microsoft.Extensions.Hosting;
using NUnit.Framework;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Fixtures)]
public abstract class BaseTest
{
    private readonly IMessageSender _sender;
    private readonly IHost _host;
    

    protected BaseTest(IMessageSender sender, Func<IHost> createReceiverHostDelegate)
    {
        _sender = sender;

        _host = createReceiverHostDelegate();
    }

    [SetUp]
    public async Task Setup()
    {
        try
        {
            await _sender.PurgeQueueAsync();

            await _host.StartAsync();
        }
        catch (Exception exception)
        {//Disposing here because Teardown is only called if Setup() is successful (see NUnit docs)
            _sender?.Dispose();
            _host?.Dispose();
        }
    }

    [TearDown]
    public void Teardown()
    {
        _sender.Dispose();

        _host.Dispose();
    }

    [Test]
    public async Task All_messages_were_processed()
    {
        await _sender.SendMessagesAsync(5, TimeSpan.FromSeconds(10));

        await _sender.SendMessagesAsync(100, TimeSpan.FromMilliseconds(200));

        await Task.Delay(TimeSpan.FromSeconds(10));
        Assert.That(() => _host.GetProcessedMessages(), Has.Count.EqualTo(_sender.SentMessages.Count).After(60).Seconds.PollEvery(3).Seconds);
        Assert.That(() => _host.GetProcessedMessages(), Is.EquivalentTo(_sender.SentMessages).After(60).Seconds.PollEvery(3).Seconds);
    }

}