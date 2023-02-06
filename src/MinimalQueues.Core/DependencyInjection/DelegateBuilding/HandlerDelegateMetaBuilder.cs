using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues.Core;

internal static class HandlerDelegateMetaBuilder
{
    internal static MessageHandlerDelegate Build(Delegate handlerDelegate, IServiceProviderIsService isService)
    {
        var delegateParameters = DelegateParametersClassifier.Classify(handlerDelegate, isService);
        
        var targetInstanceExpression = handlerDelegate.Target is { } delegateTarget
            ? Expression.Constant(delegateTarget)
            : null;

        var reflection = new CommonReflection();
        
        var messageParameter           = Expression.Parameter(reflection.MessageType);
        var nextTaskParameter          = Expression.Parameter(typeof(Func<IMessage, Task>));
        var serviceProviderParameter   = Expression.Parameter(typeof(IServiceProvider));
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

        var callParameters = delegateParameters.Select(p => p.kind switch
        {
            ParameterKind.message      => messageParameter,
            ParameterKind.service      => BuildGetServiceExpression(serviceProviderParameter, reflection, p.parameterInfo),
            ParameterKind.prop         => BuildGetPropertyExpression(messageParameter, reflection.GetPropertyMethodDefinition, p.parameterInfo),
            ParameterKind.nextFn       => nextTaskParameter,
            ParameterKind.cancellation => cancellationTokenParameter,
            _                          => throw new Exception("Unknown Parameter type")
        }).ToArray();


        var callExpression = Expression.Call(targetInstanceExpression, handlerDelegate.Method, callParameters);
        return Expression.Lambda<MessageHandlerDelegate>(callExpression
                ,messageParameter
                ,nextTaskParameter
                ,serviceProviderParameter
                ,cancellationTokenParameter)
            .Compile();
    }

    private static Expression BuildGetPropertyExpression(ParameterExpression messageParameter
        ,MethodInfo getPropertyMethodDefinition
        ,ParameterInfo parameterInfo)
    {
        var propAttribute = parameterInfo.GetCustomAttribute<PropAttribute>()!;
        var messagePropertyName = propAttribute.PropertyName;
        var getPropertyMethod = getPropertyMethodDefinition.MakeGenericMethod(parameterInfo.ParameterType);
        return Expression.Convert(Expression.Call(messageParameter
                , getPropertyMethod
                , Expression.Constant(messagePropertyName))
            , parameterInfo.ParameterType);
    }

    private static Expression BuildGetServiceExpression(ParameterExpression serviceProviderParameter
        , CommonReflection reflection
        , ParameterInfo parameterInfo)
    {
        return Expression.Convert(Expression.Call(serviceProviderParameter
                ,reflection.GetServiceMethod
                ,Expression.Constant(parameterInfo.ParameterType))
            ,parameterInfo.ParameterType);
    }
}