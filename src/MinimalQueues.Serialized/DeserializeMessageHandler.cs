using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues.Deserialization;

internal sealed class DeserializeMessageHandler
{
    private readonly IDeserializedMessageHandlerEnd[] _ends;
    private readonly MessageHandlerDelegate? _unhandledMessageEnd;

    public DeserializeMessageHandler(HandlerOptions handlerOptions, IServiceProviderIsService isService)
    {
        _ends = handlerOptions.Ends.Select(endOptions => DeserializedMessageHandlerEndMetaBuilder.Build(endOptions, handlerOptions, isService)).ToArray();
        if (handlerOptions.UnhandledMessageEndOptions is null) return;
        _unhandledMessageEnd = DeserializedMessageHandlerEndMetaBuilder.Build(handlerOptions.UnhandledMessageEndOptions, handlerOptions, isService).HandleAsync;

    }
    public async Task Handle(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token)
    {
        var handler = FindEnd(message);
        if (handler is null)
        {
            if (next is not null) await next(message);
            return;
        }
        await handler(message, next, serviceProvider, token);
    }
    private MessageHandlerDelegate? FindEnd(IMessage message)
    {
        var ends = this._ends;
        IDeserializedMessageHandlerEnd? end;
        for (var i = 0; i < ends.Length; i++)
        {
            end = ends[i];
            if (end.Match(message)) return end.HandleAsync;
        }
        return _unhandledMessageEnd;
    }
}