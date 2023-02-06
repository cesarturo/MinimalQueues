using MinimalQueues.Core;

namespace MinimalQueues;

internal sealed class DeserializedMessageHandlerEnd<TMessage> : IDeserializedMessageHandlerEnd
{
#pragma warning disable CS8618
    public Func<IMessageProperties, bool> Match { get; set; }
    public Func<IServiceProvider, BinaryData, TMessage?> Deserialize { get; set; }
    public Func<IServiceProvider, TMessage?, IMessageProperties, CancellationToken, Task> HandleDeserializedAsync { get; set; }
#pragma warning restore CS8618
    public async Task HandleAsync(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token)
    {
        var deserializedBody = Deserialize(serviceProvider, message.GetBody());
        await HandleDeserializedAsync(serviceProvider, deserializedBody, message, token);
    }
}
internal sealed class DeserializedMessageHandlerEnd : IDeserializedMessageHandlerEnd
{
#pragma warning disable CS8618
    public Func<IMessageProperties, bool> Match { get; set; }
    public Func<IServiceProvider, IMessageProperties, CancellationToken, Task> HandleDeserializedAsync { get; set; }
#pragma warning restore CS8618
    public async Task HandleAsync(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token)
    {
        await HandleDeserializedAsync(serviceProvider, message, token);
    }
}