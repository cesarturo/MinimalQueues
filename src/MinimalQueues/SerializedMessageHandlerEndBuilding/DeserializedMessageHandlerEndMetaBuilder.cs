using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues;

internal static class DeserializedMessageHandlerEndMetaBuilder
{
    internal static IDeserializedMessageHandlerEnd Build(EndOptions options
                                                       , HandlerOptions handlerOptions
                                                       , IServiceProviderIsService isService)
    {
        var endDelegateParameterInfos = EndDelegateMetadataProvider.Get(options.HandlerDelegate, isService);

        if (endDelegateParameterInfos.BodyParameter is null)
            return BuildNonGeneric(options, endDelegateParameterInfos);

        return BuildGenericUsingReflection(options, handlerOptions, endDelegateParameterInfos);
    }

    private static DeserializedMessageHandlerEnd BuildNonGeneric(EndOptions endOptions
                                                               , EndDelegateMetadata endDelegateMetadata)
    {
        var match = endOptions.Match;

        var handleDeserializedAsync = HandleDeserializeAsyncDelegateMetaBuilder.Build(endDelegateMetadata, endOptions);

        return new DeserializedMessageHandlerEnd(match, handleDeserializedAsync);
    }
    
    private static IDeserializedMessageHandlerEnd BuildGenericUsingReflection(EndOptions endOptions
                                                                            , HandlerOptions handlerOptions
                                                                            , EndDelegateMetadata endDelegateMetadata)
    {
        var methodInfo = typeof(DeserializedMessageHandlerEndMetaBuilder)
            .GetMethod(nameof(DeserializedMessageHandlerEndMetaBuilder.BuildGeneric))!
            .MakeGenericMethod(endDelegateMetadata.BodyParameter.ParameterType);

        var end = methodInfo.Invoke(null, new object[] { endOptions, handlerOptions, endDelegateMetadata })!;

        return (IDeserializedMessageHandlerEnd)end;
    }

    private static DeserializedMessageHandlerEnd<TMesssage> BuildGeneric<TMesssage>(EndOptions endOptions
                                                                                  , HandlerOptions handlerOptions
                                                                                  , EndDelegateMetadata endDelegateMetadata)
    {
        var match = endOptions.Match;

        var deserialize = DeserializeDelegateBuilder.Build<TMesssage>(endOptions, handlerOptions);

        var handleDeserializedAsync = HandleDeserializeAsyncDelegateMetaBuilder.Build<TMesssage>(endDelegateMetadata, endOptions);

        return new DeserializedMessageHandlerEnd<TMesssage>(match, deserialize, handleDeserializedAsync);
    }
}