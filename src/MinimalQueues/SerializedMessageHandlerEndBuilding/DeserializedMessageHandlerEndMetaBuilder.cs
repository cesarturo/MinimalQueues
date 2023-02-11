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
            end = new DeserializedMessageHandlerEnd();
        }
        else
        {
            var methodInfo = typeof(DeserializedMessageHandlerEndBuilder)
                .GetMethod(nameof(DeserializedMessageHandlerEndBuilder.Build))!
                .MakeGenericMethod(endDelegateParameters.BodyParameter.ParameterType);
            end = methodInfo.Invoke(null, new object[]{options, handlerOptions})!;
        }
        end.Match = options.Match;
        end.HandleDeserializedAsync = HandleDeserializeAsyncDelegateMetaBuilder.Build(endDelegateParameters, options);

        return end;
    }
}