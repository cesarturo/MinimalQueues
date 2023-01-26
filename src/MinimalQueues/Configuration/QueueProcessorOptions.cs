namespace MinimalQueues;

public sealed class QueueProcessorOptions
{
    public IQueueConnection? Connection { get; set; }
    public List<MessageHandlerDelegate> MessageHandlerDelegates { get; set; } = new();
}