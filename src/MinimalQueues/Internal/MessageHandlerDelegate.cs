namespace MinimalQueues;

public delegate Task MessageHandlerDelegate(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token);