using MinimalQueues.Core;

namespace MinimalQueues.AwsLambdaSqs;

public sealed class AwsLambdaSqsConnection: IQueueConnection
{
    public Action<Exception>? OnError { get; set; }

    private Func<IMessage, CancellationToken, Task>? _processMessageDelegate;

    public Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken)
    {
        _processMessageDelegate = processMessageDelegate;
        return Task.CompletedTask;
    }
    
    internal async Task ProcessMessage(LambdaSqsMessage message, CancellationToken cancellation)
    {
        try
        {
            await _processMessageDelegate!(message, cancellation);
        }
        catch (Exception exception)
        {
            OnError?.Invoke(exception);
            throw;
        }
    }
}