using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.Core;

namespace MinimalQueues;

internal static class EndDelegateParameterClassifier
{
    internal static EndDelegateParameters Classify(Delegate handlerDelegate, IServiceProviderIsService isService)
    {
        var parametersClassified = handlerDelegate.Method.GetParameters()
            .Select(parameterInfo => GetParameterWithType(parameterInfo, isService))
            .ToArray();
        var bodyParameter = parametersClassified.FirstOrDefault(itm => itm.type == ParameterKind.body);
        return new EndDelegateParameters(parametersClassified, bodyParameter.parameterInfo);
    }

    private static (ParameterInfo parameterInfo, ParameterKind type) GetParameterWithType(ParameterInfo parameterInfo, IServiceProviderIsService isService)
    {
        return (parameterInfo, type: parameterInfo switch
        {
            _ when isService.IsService(parameterInfo.ParameterType)         => ParameterKind.service,
            _ when parameterInfo.GetCustomAttribute<PropAttribute>() is { } => ParameterKind.prop,
            _ when parameterInfo.ParameterType == typeof(CancellationToken) => ParameterKind.cancellation,
            _                                                               => ParameterKind.body
        });
    }
}