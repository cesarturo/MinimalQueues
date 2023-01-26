using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues;

internal static class DelegateParametersClassifier
{
    internal static (ParameterInfo parameterInfo, ParameterKind kind)[] Classify(Delegate handlerDelegate, IServiceProviderIsService isService)
    {
        var commonReflection = new CommonReflection();
        (ParameterInfo parameterInfo, ParameterKind kind)[] parametersClassified = 
            handlerDelegate.Method.GetParameters()
            .Select(parameterInfo => GetParameterWithType(parameterInfo, isService, commonReflection))
            .ToArray();
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