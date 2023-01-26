namespace MinimalQueues;

public interface IQueueConnection
{
    Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken);
}