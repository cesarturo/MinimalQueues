using Microsoft.Extensions.Hosting;
using NUnit.Framework;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Fixtures)]
public abstract class BaseTest
{
    private readonly IMessageSender _sender;
    private readonly Func<IHost> _createReceiverHostDelegate;
    

    protected BaseTest(IMessageSender sender, Func<IHost> createReceiverHostDelegate)
    {
        _sender = sender;
        _createReceiverHostDelegate = createReceiverHostDelegate;
    }

    [SetUp]
    public async Task Setup()
    {
        try
        {
            await _sender.PurgeQueueAsync();
        }
        catch (Exception exception)
        {//Disposing here because Teardown is only called if Setup() is successful (see NUnit docs)
            _sender?.Dispose();
        }
    }

    [TearDown]
    public void Teardown()
    {
        _sender.Dispose();
    }

    [Test]
    public async Task All_messages_are_processed()
    {
        using var receiverHost = await StartNewReceiverHost();

        await _sender.SendMessagesAsync(count: 5, TimeSpan.FromSeconds(10));

        await _sender.SendMessagesAsync(count: 100, TimeSpan.FromMilliseconds(200));

        await Task.Delay(TimeSpan.FromSeconds(10));

        Assert.That(() => receiverHost.GetProcessedMessages(), Has.Count.EqualTo(_sender.SentMessages.Count).After(60).Seconds.PollEvery(3).Seconds);

        Assert.That(() => receiverHost.GetProcessedMessages(), Is.EquivalentTo(_sender.SentMessages).After(60).Seconds.PollEvery(3).Seconds);
    }

    [Test]
    public async Task Stops_processing_messages()
    {
        using var receiverHost = await StartNewReceiverHost();

        await _sender.SendMessagesAsync(count: 20);

        await Wait(seconds: 5);

        await receiverHost.StopAsync();

        var expected = receiverHost.GetProcessedMessages().ToArray();

        await _sender.SendMessagesAsync(count: 5);

        await Wait(seconds: 5);

        Assert.That(() => receiverHost.GetProcessedMessages(), Is.EquivalentTo(expected));

        Task Wait(int seconds) => Task.Delay(TimeSpan.FromSeconds(seconds));
    }

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