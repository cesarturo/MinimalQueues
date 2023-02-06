using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues;

internal static class DeserializedMessageHandlerEndMetaBuilder
{
    internal static IDeserializedMessageHandlerEnd Build(EndOptions options, HandlerOptions handlerOptions,
        IServiceProviderIsService isService)
    {
        var endDelegateParameters = EndDelegateParameterClassifier.Classify(options.HandlerDelegate, isService);

        dynamic end;

        if (endDelegateParameters.BodyParameter is null)
        {
            var endType = typeof(DeserializedMessageHandlerEnd);
            end = endType.GetConstructor(Type.EmptyTypes)!.Invoke(null);
        }
        else
        {
            var endType = typeof(DeserializedMessageHandlerEnd<>).MakeGenericType(endDelegateParameters.BodyParameter.ParameterType);
            end = endType.GetConstructor(Type.EmptyTypes)!.Invoke(null);
            end.Deserialize = DeserializeDelegateMetaBuilder.Build(endDelegateParameters, options, handlerOptions);
        }
        end.Match = options.Match;
        end.HandleDeserializedAsync = HandleDeserializeAsyncDelegateMetaBuilder.Build(endDelegateParameters, options);

        return end;
    }
}