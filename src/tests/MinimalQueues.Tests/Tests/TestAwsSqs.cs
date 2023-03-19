using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalQueues;
using MinimalQueues.AwsSqs;
using MinimalSqsClient;
using NUnit.Framework;

[TestFixture]
public class TestAwsSqs
{
    private IHost host;
    private string[] messages;
    private SqsClient sqsClient;
    [OneTimeSetUp]
    public async Task Setup()
    {
        sqsClient = new SqsClient(new SqsClientOptions
        {
            QueueUrl = Environment.GetEnvironmentVariable("QUEUE_URL")!//TestContext.Parameters["QueueUrl"]
        });
        var purgeTask = sqsClient.PurgeQueueAsync();

        CreateHost();

        await purgeTask;

        await host.StartAsync();
    }

    private void CreateHost()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        var queueApp = hostBuilder
            .ConfigureServices(services => services.AddSingleton<ConcurrentBag<string>>())
            .AddAwsSqsListener(queueUrl: Environment.GetEnvironmentVariable("QUEUE_URL") //TestContext.Parameters["QueueUrl"]
                , prefetchCount: 20
                , visibilityTimeout: 5
                , renewVisibilityTime: 4
                , maxConcurrency: 4)
            .UseDeserializedMessageHandler();

        queueApp.Use((string message, ConcurrentBag<string> receivedMessages) =>
        {
            receivedMessages.Add(message);
            return Task.CompletedTask;
        });

        host = hostBuilder.Build();
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        await host.StopAsync();
    }

    private async Task SendMessages(string[] messages)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        var groups = Enumerable.Range(0, messages.Length).GroupBy(i => i / 10);
        foreach (var group in groups)
        {
            if (group.Key is 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            var batch = group.Select(i => JsonSerializer.Serialize(messages[i])).ToArray();
            await sqsClient.SendMessageBatchAsync(batch);
        }
    }

    [Test]
    public async Task All_messages_were_processed()
    {
        messages = Enumerable.Range(0, 100).Select(i => $"testmessage-{i}").ToArray();

        await SendMessages(messages);

        var receivedMessages = host.Services.GetRequiredService<ConcurrentBag<string>>();

        Assert.That(() => receivedMessages, Is.EquivalentTo(messages).After(30).Seconds.PollEvery(2).Seconds);
    }
}
