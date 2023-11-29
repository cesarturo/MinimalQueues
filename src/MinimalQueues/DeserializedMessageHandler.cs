using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.Core;

namespace MinimalQueues;

internal sealed class DeserializedMessageHandler
{
    private readonly IMessageHandlerEndpoint[] _endpoints;
    private readonly MessageHandlerDelegate? _unhandledMessageEnd;

    public DeserializedMessageHandler(HandlerOptions handlerOptions, IServiceProviderIsService isService)
    {
        _endpoints = handlerOptions.EndpointsOptions.Select(endOptions => MessageHandlerEndpointBuilder.Build(endOptions, handlerOptions, isService)).ToArray();
        if (handlerOptions.UnhandledMessageEndpointOptions is null) return;
        _unhandledMessageEnd = MessageHandlerEndpointBuilder.Build(handlerOptions.UnhandledMessageEndpointOptions, handlerOptions, isService).HandleAsync;
    }
    public async Task Handle(IMessage message, Func<IMessage, Task>? next, IServiceProvider serviceProvider, CancellationToken token)
    {
        var handler = FindEndpoint(message);
        if (handler is null)
        {
            if (next is not null) await next(message);
            return;
        }
        await handler(message, next, serviceProvider, token);
    }
    private MessageHandlerDelegate? FindEndpoint(IMessage message)
    {
        var endpoints = _endpoints;
        IMessageHandlerEndpoint? end;
        for (var i = 0; i < endpoints.Length; i++)
        {
            end = endpoints[i];
            if (end.Match(message)) return end.HandleAsync;
        }
        return _unhandledMessageEnd;
    }
}