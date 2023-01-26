namespace MinimalQueues.Deserialization;

internal interface IDeserializedMessageHandlerEnd
{
    Func<IMessageProperties, bool> Match { get; set; }
    Task HandleAsync(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token);
}