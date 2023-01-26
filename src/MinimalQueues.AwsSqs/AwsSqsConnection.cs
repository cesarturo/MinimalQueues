using System.Threading.Channels;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace MinimalQueues.AwsSqs;

internal sealed class AwsSqsConnection : IQueueConnection, IAwsSqsConnectionConfiguration, IDisposable
{
#pragma warning disable CS8618
    public RegionEndpoint? Region { get; set; }
    public AWSCredentials? Credentials { get; set; }
    public AmazonSQSConfig? ClientConfig { get; set; }
    public string QueueUrl { get; set; }
    public int WaitTimeSeconds { get; set; } = 4;
    public int VisibilityTimeout { get; set; } = 30;
    public int RenewVisibilityWaitTime { get; set; } = 24;
    public int MaxConcurrentCalls { get; set; }
    public int PrefetchCount { get; set; }
    public int RequestMaxNumberOfMessages { get; set; } = 10;
    public Func<int, TimeSpan>? BackOffFunction { get; set; }
    public Action<Exception>? OnError { get; set; }

    private Func<IMessage, CancellationToken, Task> _processMessageAsync;
    internal AmazonSQSClient _sqsClient;
    private CancellationTokenSource _cancellation;
    internal CancellationToken Cancellation => _cancellation.Token;

#pragma warning restore CS8618
    public Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken)
    {
        Validate();
        SetDefaults();
        _processMessageAsync = processMessageDelegate;
        _sqsClient = GetClient();
        _cancellation = new CancellationTokenSource();
        IMessageReceiver messageReceiver = PrefetchCount is 0 ? new OnDemandMessageReceiver(this)
                                                              : new PrefetchMessageReceiver(this);
        var workerTasks = Enumerable.Range(1, MaxConcurrentCalls).Select(i => new Worker(this, messageReceiver).Start()).ToArray();
        return Task.CompletedTask;
    }

    private void Validate()
    {
        if (VisibilityTimeout <= RenewVisibilityWaitTime)
        {
            throw new Exception($"{nameof(VisibilityTimeout)} must be greater thant {nameof(RenewVisibilityWaitTime)}.");
        }
    }
    private void SetDefaults()
    {
        BackOffFunction ??= i => TimeSpan.FromSeconds(2);
    }


    private AmazonSQSClient GetClient()
    {
        if (Credentials is not null)
        {
            if (ClientConfig is not null) return new AmazonSQSClient(Credentials, ClientConfig);
            if (Region is not null) return new AmazonSQSClient(Credentials, Region);
            return new AmazonSQSClient(Credentials);
        }
        if (ClientConfig is not null) return new AmazonSQSClient(ClientConfig);
        if (Region is not null) return new AmazonSQSClient(Region);
        return new AmazonSQSClient();
    }

    internal async Task ProcessMessageAsync(Message message)
    {
        await _processMessageAsync(new SqsMessage(message), Cancellation);
    }
    internal async Task DeleteMessage(Message message)
    {
        await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
        {
            QueueUrl = QueueUrl,
            ReceiptHandle = message.ReceiptHandle
        });
    }

    internal async Task UpdateVisibility(Message message)
    {
        await _sqsClient.ChangeMessageVisibilityAsync(QueueUrl, message.ReceiptHandle, VisibilityTimeout);
    }


    public void Dispose()
    {
        _cancellation?.Cancel();
        _sqsClient?.Dispose();
    }
}

public interface IAwsSqsConnectionConfiguration
{
    RegionEndpoint? Region { get; set; }
    AWSCredentials? Credentials { get; set; }
    AmazonSQSConfig? ClientConfig { get; set; }
    string QueueUrl { get; set; }
    int WaitTimeSeconds { get; set; }
    int VisibilityTimeout { get; set; }
    int RenewVisibilityWaitTime { get; set; }

    int MaxConcurrentCalls { get; set; }
    int PrefetchCount { get; set; }
    int RequestMaxNumberOfMessages { get; set; }
    Func<int, TimeSpan> BackOffFunction { get; set; }
    Action<Exception> OnError { get; set; }

}