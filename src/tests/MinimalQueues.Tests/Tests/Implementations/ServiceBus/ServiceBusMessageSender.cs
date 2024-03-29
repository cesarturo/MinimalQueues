﻿using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Tests.Internal;

namespace Tests.Implementations.ServiceBus;

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

    public async Task<bool> PurgeQueueAsync()
    {
        await using var receiver = _client.CreateReceiver(_topic, _subscription, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });
        IReadOnlyList<ServiceBusReceivedMessage>? batch;
        do
        {
            batch = await receiver.ReceiveMessagesAsync(100, TimeSpan.FromSeconds(2));
        } while (batch.Count > 0);

        return true;
    }

    public async Task SendMessagesAsync(int count, TimeSpan? executionTimeHeader)
    {
        var batches = Enumerable.Range(0, count).GroupBy(i => i % 10, i => GenerateMessage());

        foreach (var batch in batches)
        {
            var batchToSend = batch.Select(message =>
            {
                var busMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
                if (executionTimeHeader.HasValue)
                    busMessage.ApplicationProperties["execution-time"] = executionTimeHeader.ToString();
                return busMessage;
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