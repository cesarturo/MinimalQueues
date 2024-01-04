using System.Text.Json;
using Azure;
using Azure.Identity;
using Azure.Messaging.ServiceBus;

public class ServiceBusMessageSender : IMessageSender
{
    private readonly string _topic;
    private readonly string _subscription;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private int i;
    public ServiceBusMessageSender(string serviceBusNamespace, string topic, string subscription)
    {
        _topic = topic;
        _subscription = subscription;
        _client = new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential());
        _sender = _client.CreateSender(topic);
    }

    public List<string> SentMessages { get; } = new();

    public async Task PurgeQueueAsync()
    {
        await using var receiver = _client.CreateReceiver(_topic, _subscription, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });
        IReadOnlyList<ServiceBusReceivedMessage>? batch;
        do
        {
            batch = await receiver.ReceiveMessagesAsync(100, TimeSpan.FromSeconds(2));
        }while (batch.Count > 0);
    }

    public async Task SendMessagesAsync(int count, TimeSpan executionTimeHeader)
    {
        var batches = Enumerable.Range(0, count).GroupBy(i => i % 10, i => GenerateMessage());

        foreach (var batch in batches)
        {
            var batchToSend = batch.Select(message => new ServiceBusMessage(JsonSerializer.Serialize(message))
            {
                ApplicationProperties = { ["execution-time"] = executionTimeHeader.ToString() }
            }).ToList();
            await _sender.SendMessagesAsync(batchToSend);
            SentMessages.AddRange(batch);
        }

        string GenerateMessage()
        {
            Interlocked.Increment(ref i);
            return $"testmessage-{i}";
        }
    }
    public void Dispose()
    {
        _client.DisposeAsync().GetAwaiter().GetResult();
    }
}