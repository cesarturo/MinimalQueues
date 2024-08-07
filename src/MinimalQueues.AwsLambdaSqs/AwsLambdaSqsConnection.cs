using MinimalQueues.Core;

namespace MinimalQueues.AwsLambdaSqs;

internal sealed class AwsLambdaSqsConnection: IQueueConnection, IAwsLambdaSqsConnectionConfiguration
{
    public string? QueueArn { get; set; }

    public Action<Exception>? OnError { get; set; }

    private CancellationToken _cancellation;


    public Func<IMessage, CancellationToken, Task> ProcessMessageDelegate { private get; set; }

    public Task Start(CancellationToken cancellationToken)
    {
        _cancellation = cancellationToken;
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    internal async Task ProcessMessage(LambdaSqsMessage message)
    {
        try
        {
            await ProcessMessageDelegate!(message, _cancellation);
        }
        catch (Exception exception)
        {
            OnError?.Invoke(exception);
            throw;
        }
    }
}