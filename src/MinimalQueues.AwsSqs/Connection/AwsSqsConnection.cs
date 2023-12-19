using MinimalQueues.AwsSqs.Connection.Internal;
using MinimalQueues.AwsSqs.Connection.Internal.OnDemand;
using MinimalQueues.AwsSqs.Connection.Internal.Prefetch;
using MinimalQueues.Core;
using MinimalSqsClient;

namespace MinimalQueues.AwsSqs.Connection;

internal sealed class AwsSqsConnection : IQueueConnection, IDisposable
{
#pragma warning disable CS8618
    internal readonly AwsSqsConnectionConfiguration Configuration;

    internal ISqsClient _sqsClient;
    private CancellationTokenSource _cancellation;
    internal CancellationToken Cancellation => _cancellation.Token;
    private IMessageReceiver _messageReceiver;
    private Task[] _workerTasks;

    public AwsSqsConnection(ISqsClient sqsClient, AwsSqsConnectionConfiguration configuration)
    {
        _sqsClient = sqsClient;
        Configuration = configuration;
    }

#pragma warning restore CS8618
    public Func<IMessage, CancellationToken, Task> ProcessMessageDelegate { private get; set; }

    public Task Start(CancellationToken cancellationToken)
    {
        Validate();
        SetDefaults();
        _cancellation = new CancellationTokenSource();
        _messageReceiver = Configuration.PrefetchCount is 0 ? new OnDemandMessageReceiver(this)
                                              : new PrefetchMessageReceiver(this);
        _workerTasks = Enumerable.Range(1, Configuration.MaxConcurrency).Select(i => new Worker(this, _messageReceiver).Start()).ToArray();
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        _cancellation.Cancel();
        await Task.WhenAll(_workerTasks);
    }

    private void Validate()
    {
        if (_workerTasks is not null && _workerTasks.All(task => task.IsCompleted))
            throw new Exception("Connection workers are running");

        if (Configuration.VisibilityTimeout <= Configuration.RenewVisibilityWaitTime)
            throw new Exception($"{nameof(Configuration.VisibilityTimeout)} must be greater thant {nameof(Configuration.RenewVisibilityWaitTime)}.");
    }
    private void SetDefaults()
    {
        Configuration.BackOffFunction ??= _ => TimeSpan.FromSeconds(2);
    }

    internal Task ProcessMessageAsync(SqsMessage message)
    {
        return ProcessMessageDelegate(message, Cancellation);
    }
    internal Task DeleteMessageAsync(SqsMessage message)
    {
        return _sqsClient.DeleteMessageAsync(message.InternalMessage.ReceiptHandle);
    }

    internal Task UpdateVisibilityAsync(MinimalSqsClient.SqsMessage message)
    {
        return _sqsClient.ChangeMessageVisibilityAsync(message.ReceiptHandle, Configuration.VisibilityTimeout);
    }

    internal Task UpdateVisibilityBatchAsync(MinimalSqsClient.SqsMessage[] requestEntries)
    {
        var receiptHandles = requestEntries.Select(m => m.ReceiptHandle).ToArray();
        return _sqsClient.ChangeMessageVisibilityBatchAsync(receiptHandles, Configuration.VisibilityTimeout);
    }

    internal Task AbandonMessageAsync(SqsMessage message)
    {
        return _sqsClient.ChangeMessageVisibilityAsync(message.InternalMessage.ReceiptHandle, 0);
    }


    public void Dispose()
    {
        _cancellation?.Cancel();
        _messageReceiver.Dispose();
    }
}