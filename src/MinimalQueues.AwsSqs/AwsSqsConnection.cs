using Amazon;
using Amazon.Runtime;
using MinimalQueues.Core;
using MinimalQueues.Core.AwsSqs;
using MinimalSqsClient;

namespace MinimalQueues.AwsSqs;

internal sealed class AwsSqsConnection : IQueueConnection, IDisposable
{
#pragma warning disable CS8618
    internal readonly AwsSqsConnectionConfiguration Configuration;

    private Func<IMessage, CancellationToken, Task> _processMessageAsync;
    internal ISqsClient _sqsClient;
    private CancellationTokenSource _cancellation;
    internal CancellationToken Cancellation => _cancellation.Token;
    private IMessageReceiver _messageReceiver;

    public AwsSqsConnection(ISqsClient sqsClient, AwsSqsConnectionConfiguration configuration)
    {
        _sqsClient = sqsClient;
        Configuration = configuration;
    }

#pragma warning restore CS8618
    public Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken)
    {
        Validate();
        SetDefaults();
        _processMessageAsync = processMessageDelegate;
        _cancellation = new CancellationTokenSource();
        _messageReceiver = Configuration.PrefetchCount is 0 ? new OnDemandMessageReceiver(this)
                                              : new PrefetchMessageReceiver(this);
        var workerTasks = Enumerable.Range(1, Configuration.MaxConcurrentCalls).Select(i => new Worker(this, _messageReceiver).Start()).ToArray();
        return Task.CompletedTask;
    }

    private void Validate()
    {
        if (Configuration.VisibilityTimeout <= Configuration.RenewVisibilityWaitTime)
        {
            throw new Exception($"{nameof(Configuration.VisibilityTimeout)} must be greater thant {nameof(Configuration.RenewVisibilityWaitTime)}.");
        }
    }
    private void SetDefaults()
    {
        Configuration.BackOffFunction ??= i => TimeSpan.FromSeconds(2);
    }

    internal Task ProcessMessageAsync(SqsMessage message)
    {
        return _processMessageAsync(message, Cancellation);
    }
    internal Task DeleteMessage(SqsMessage message)
    {
        return _sqsClient.DeleteMessageAsync(message.InternalMessage.ReceiptHandle);
    }

    internal Task UpdateVisibility(MinimalSqsClient.SqsMessage message)
    {
        return _sqsClient.ChangeMessageVisibilityAsync(message.ReceiptHandle, Configuration.VisibilityTimeout);
    }


    public void Dispose()
    {
        _cancellation?.Cancel();
        _messageReceiver.Dispose();
    }
}

public sealed class AwsSqsConnectionConfiguration
{
    public RegionEndpoint? Region { get; set; }
    public AWSCredentials? Credentials { get; set; }
    public string QueueUrl { get; set; }
    public int WaitTimeSeconds { get; set; }
    public int VisibilityTimeout { get; set; }
    public int RenewVisibilityWaitTime { get; set; }
    
    public int MaxConcurrentCalls { get; set; }
    public int PrefetchCount { get; set; }
    public int RequestMaxNumberOfMessages { get; set; }
    public Func<int, TimeSpan> BackOffFunction { get; set; }
    public Action<Exception> OnError { get; set; }

}
