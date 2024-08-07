namespace MinimalQueues.Core;

public interface IQueueConnection
{
    public Func<IMessage, CancellationToken, Task>? ProcessMessageDelegate { set; }
    Task Start(CancellationToken cancellationToken);
    Task Stop();
}