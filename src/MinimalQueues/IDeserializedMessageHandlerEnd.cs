using MinimalQueues.Core;

namespace MinimalQueues;

internal interface IDeserializedMessageHandlerEnd
{
    Func<IMessageProperties, bool> Match { get; }
    Task HandleAsync(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token);
}