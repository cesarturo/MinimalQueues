namespace MinimalQueues.Core;

public interface IQueueConnection
{
    Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken);
}