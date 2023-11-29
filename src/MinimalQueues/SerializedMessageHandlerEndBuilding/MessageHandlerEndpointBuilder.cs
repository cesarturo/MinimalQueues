using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues;

internal static class MessageHandlerEndpointBuilder
{
    internal static IMessageHandlerEndpoint Build(EndpointOptions options
                                                 ,HandlerOptions handlerOptions
                                                 ,IServiceProviderIsService isService)
    {
        var endDelegateParameterInfos = EndDelegateMetadataProvider.Get(options.HandlerDelegate, isService);

        if (endDelegateParameterInfos.BodyParameter is null)
            return BuildWithoutMessageParameter(options, endDelegateParameterInfos);

        return BuildWithMessageParameter(options, handlerOptions, endDelegateParameterInfos);
    }

    private static MessageHandlerEndpoint BuildWithoutMessageParameter(EndpointOptions endpointOptions
                                                                      ,EndDelegateMetadata endDelegateMetadata)
    {
        var match = endpointOptions.Match;

        var handleAsync = HandleAsyncDelegateMetaBuilder.Build(endDelegateMetadata, endpointOptions);

        return new MessageHandlerEndpoint(match, handleAsync);
    }
    
    private static IMessageHandlerEndpoint BuildWithMessageParameter(EndpointOptions endpointOptions
                                                                    ,HandlerOptions handlerOptions
                                                                    ,EndDelegateMetadata endDelegateMetadata)
    {
        var methodInfo = typeof(MessageHandlerEndpointBuilder)
            .GetMethod(nameof(MessageHandlerEndpointBuilder.BuildWithMessageParameterInternal), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(endDelegateMetadata.BodyParameter.ParameterType);

        var end = methodInfo.Invoke(null, new object[] { endpointOptions, handlerOptions, endDelegateMetadata })!;

        return (IMessageHandlerEndpoint)end;
    }

    private static MessageHandlerEndpoint<TMesssage> BuildWithMessageParameterInternal<TMesssage>(
                                                                      EndpointOptions endpointOptions
                                                                     ,HandlerOptions handlerOptions
                                                                     ,EndDelegateMetadata endDelegateMetadata)
    {
        var match = endpointOptions.Match;

        var deserialize = DeserializeDelegateBuilder.Build<TMesssage>(endpointOptions, handlerOptions);

        var handleDeserializedAsync = HandleAsyncDelegateMetaBuilder.Build<TMesssage>(endDelegateMetadata, endpointOptions);

        return new MessageHandlerEndpoint<TMesssage>(match, deserialize, handleDeserializedAsync);
    }
}