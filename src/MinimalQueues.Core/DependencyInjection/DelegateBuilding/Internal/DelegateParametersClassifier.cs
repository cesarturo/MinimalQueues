using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues.Core;

internal static class DelegateParametersClassifier
{
    internal static IEnumerable<(ParameterInfo parameterInfo, ParameterKind kind)> Classify(Delegate handlerDelegate, IServiceProviderIsService isService)
    {
        var parametersClassified = handlerDelegate.Method.GetParameters()
            .Select(parameterInfo => GetParameterWithType(parameterInfo, isService, new CommonReflection()));
        return parametersClassified;
    }

    private static (ParameterInfo parameterInfo, ParameterKind kind) GetParameterWithType(ParameterInfo parameterInfo,
        IServiceProviderIsService isService, CommonReflection commonReflection)
    {
        return (parameterInfo, kind: parameterInfo switch
        {
            _ when isService.IsService(parameterInfo.ParameterType)            => ParameterKind.service,
            _ when parameterInfo.ParameterType == commonReflection.MessageType => ParameterKind.message,
            _ when parameterInfo.GetCustomAttribute<PropAttribute>() is { }    => ParameterKind.prop,
            _ when parameterInfo.ParameterType == typeof(Func<IMessage, Task>) => ParameterKind.nextFn,
            _ when parameterInfo.ParameterType == typeof(CancellationToken)    => ParameterKind.cancellation,
            _                                                                  => throw new Exception($"Parameter {parameterInfo.Name} of type {parameterInfo.ParameterType} cannot be resolved.")
        });
    }
}