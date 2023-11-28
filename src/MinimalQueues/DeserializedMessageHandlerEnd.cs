﻿using MinimalQueues.Core;

namespace MinimalQueues;

internal sealed class DeserializedMessageHandlerEnd<TMessage> : IDeserializedMessageHandlerEnd
{
    public DeserializedMessageHandlerEnd(Func<IMessageProperties, bool> match
                                       , Func<IServiceProvider, BinaryData, TMessage?> deserialize
                                       , Func<IServiceProvider, TMessage?, IMessageProperties, CancellationToken, Task> handleDeserializedAsync)
    {
        Match = match;
        Deserialize = deserialize;
        HandleDeserializedAsync = handleDeserializedAsync;
    }
    public Func<IMessageProperties, bool> Match { get; }
    public Func<IServiceProvider, BinaryData, TMessage?> Deserialize { get; }
    public Func<IServiceProvider, TMessage?, IMessageProperties, CancellationToken, Task> HandleDeserializedAsync { get; }

    public async Task HandleAsync(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token)
    {
        var deserializedBody = Deserialize(serviceProvider, message.GetBody());
        await HandleDeserializedAsync(serviceProvider, deserializedBody, message, token);
    }
}
internal sealed class DeserializedMessageHandlerEnd : IDeserializedMessageHandlerEnd
{
    public DeserializedMessageHandlerEnd(Func<IMessageProperties, bool> match
                                       , Func<IServiceProvider, IMessageProperties, CancellationToken, Task> handleDeserializedAsync)
    {
        Match = match;
        HandleDeserializedAsync = handleDeserializedAsync;
    }
    public Func<IMessageProperties, bool> Match { get; }
    public Func<IServiceProvider, IMessageProperties, CancellationToken, Task> HandleDeserializedAsync { get; }

    public async Task HandleAsync(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token)
    {
        await HandleDeserializedAsync(serviceProvider, message, token);
    }
}