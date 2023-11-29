using MinimalQueues.Core;

namespace MinimalQueues;

internal interface IMessageHandlerEndpoint
{
    Func<IMessageProperties, bool> Match { get; }
    Task HandleAsync(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token);
}