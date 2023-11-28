using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues;

internal static class DeserializedMessageHandlerEndMetaBuilder
{
    internal static IDeserializedMessageHandlerEnd Build(EndOptions options
                                                       , HandlerOptions handlerOptions
                                                       , IServiceProviderIsService isService)
    {
        var endDelegateParameters = EndDelegateParameterClassifier.Classify(options.HandlerDelegate, isService);

        if (endDelegateParameters.BodyParameter is null)
            return BuildNonGeneric(options, endDelegateParameters);

        return BuildGenericUsingReflection(options, handlerOptions, endDelegateParameters);
    }

    private static DeserializedMessageHandlerEnd BuildNonGeneric(EndOptions endOptions
                                                               , EndDelegateParameters endDelegateParameters)
    {
        var match = endOptions.Match;

        var handleDeserializedAsync = HandleDeserializeAsyncDelegateMetaBuilder.Build(endDelegateParameters, endOptions);

        return new DeserializedMessageHandlerEnd(match, handleDeserializedAsync);
    }
    
    private static IDeserializedMessageHandlerEnd BuildGenericUsingReflection(EndOptions endOptions
                                                                            , HandlerOptions handlerOptions
                                                                            , EndDelegateParameters endDelegateParameters)
    {
        var methodInfo = typeof(DeserializedMessageHandlerEndMetaBuilder)
            .GetMethod(nameof(DeserializedMessageHandlerEndMetaBuilder.BuildGeneric))!
            .MakeGenericMethod(endDelegateParameters.BodyParameter.ParameterType);

        var end = methodInfo.Invoke(null, new object[] { endOptions, handlerOptions, endDelegateParameters })!;

        return (IDeserializedMessageHandlerEnd)end;
    }

    private static DeserializedMessageHandlerEnd<TMesssage> BuildGeneric<TMesssage>(EndOptions endOptions
                                                                                  , HandlerOptions handlerOptions
                                                                                  , EndDelegateParameters endDelegateParameters)
    {
        var match = endOptions.Match;

        var deserialize = DeserializeDelegateBuilder.Build<TMesssage>(endOptions, handlerOptions);

        var handleDeserializedAsync = HandleDeserializeAsyncDelegateMetaBuilder.Build<TMesssage>(endDelegateParameters, endOptions);

        return new DeserializedMessageHandlerEnd<TMesssage>(match, deserialize, handleDeserializedAsync);
    }
}