using MinimalQueues.Core;

namespace MinimalQueues.AwsLambdaSqs;

internal sealed class AwsLambdaSqsConnection: IQueueConnection, IAwsLambdaSqsConnectionConfiguration
{
    public string? QueueArn { get; set; }

    public Action<Exception>? OnError { get; set; }

    private Func<IMessage, CancellationToken, Task>? _processMessageDelegate;

    private CancellationToken _cancellation;
         

    public Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken)
    {
        _processMessageDelegate = processMessageDelegate;
        _cancellation = cancellationToken;
        return Task.CompletedTask;
    }
    
    internal async Task ProcessMessage(LambdaSqsMessage message)
    {
        try
        {
            await _processMessageDelegate!(message, _cancellation);
        }
        catch (Exception exception)
        {
            OnError?.Invoke(exception);
            throw;
        }
    }
}