using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.Core;

namespace MinimalQueues;

internal static class EndDelegateMetadataProvider
{
    internal static EndDelegateMetadata Get(Delegate handlerDelegate, IServiceProviderIsService isService)
    {
        var endDelegateParameterInfos = handlerDelegate.Method.GetParameters()
            .Select(parameterInfo => GetEndDelegateParameterInfos(parameterInfo, isService))
            .ToArray();
        EndDelegateParameterInfo bodyParameter;
        try
        {
            bodyParameter = endDelegateParameterInfos.SingleOrDefault(itm => itm.Type == ParameterKind.body);
        }
        catch (InvalidOperationException)
        {
            throw CreateMoreThanOneBodyParameterException(handlerDelegate);
        }
        return new EndDelegateMetadata(endDelegateParameterInfos, bodyParameter.ParameterInfo);
    }
    private static Exception CreateMoreThanOneBodyParameterException(Delegate handlerDelegate)
        => new($"Handler delegate {handlerDelegate.Method} contains more than one body parameter.");

    private static EndDelegateParameterInfo GetEndDelegateParameterInfos(ParameterInfo parameterInfo, IServiceProviderIsService isService)
    {
        return new EndDelegateParameterInfo(parameterInfo, type: parameterInfo switch
        {
            _ when isService.IsService(parameterInfo.ParameterType)         => ParameterKind.service,
            _ when parameterInfo.GetCustomAttribute<PropAttribute>() is { } => ParameterKind.prop,
            _ when parameterInfo.ParameterType == typeof(CancellationToken) => ParameterKind.cancellation,
            _                                                               => ParameterKind.body
        });
    }
}

