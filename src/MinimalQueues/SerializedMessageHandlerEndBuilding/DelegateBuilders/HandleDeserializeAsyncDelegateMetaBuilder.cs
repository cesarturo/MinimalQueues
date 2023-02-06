using System.Linq.Expressions;
using System.Reflection;
using MinimalQueues.Core;

namespace MinimalQueues;

internal static class HandleDeserializeAsyncDelegateMetaBuilder
{
    internal static dynamic Build(EndDelegateParameters endDelegateParameters, EndOptions options)
    {
        bool buildWithMessageParameter = endDelegateParameters.BodyParameter is not null;

        var targetInstanceExpression = options.HandlerDelegate.Target is { } delegateTarget
            ? Expression.Constant(delegateTarget)
            : null;
        var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider));
        var messagePropertiesParameter = Expression.Parameter(typeof(IMessageProperties));
        var deserializedMessageParameter = buildWithMessageParameter
            ? Expression.Parameter(endDelegateParameters.BodyParameter!.ParameterType)
            : null;
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

        var reflection = new CommonReflection();
        var parameters = endDelegateParameters.ParametersClassified.Select(itm =>
        {
            var parameterInfo = itm.parameterInfo;
            return itm.type switch
            {
                ParameterKind.service      => BuildGetServiceExpression(serviceProviderParameter, reflection, parameterInfo),
                ParameterKind.prop         => BuildGetPropertyExpression(messagePropertiesParameter, reflection.GetPropertyMethodDefinition, parameterInfo),
                ParameterKind.body         => deserializedMessageParameter!,
                ParameterKind.cancellation => cancellationTokenParameter,
                _                          => throw new Exception("Unknown Parameter type")
            };
        }).ToArray();

        var handlerDelegateInvokationExpression =
            Expression.Call(targetInstanceExpression, options.HandlerDelegate.Method, parameters);

        var lambdaParameters = new List<ParameterExpression>(buildWithMessageParameter ? 4 : 3);
        lambdaParameters.Add(serviceProviderParameter);
        if (buildWithMessageParameter) lambdaParameters.Add(deserializedMessageParameter!);
        lambdaParameters.Add(messagePropertiesParameter);
        lambdaParameters.Add(cancellationTokenParameter);

        return Expression.Lambda(handlerDelegateInvokationExpression, lambdaParameters).Compile();
    }

    private static Expression BuildGetPropertyExpression(ParameterExpression messageVariable,
        MethodInfo getPropertyMethodDefinition, ParameterInfo parameterInfo)
    {
        var propAttribute = parameterInfo.GetCustomAttribute<PropAttribute>()!;
        var messagePropertyName = propAttribute.PropertyName;
        var getPropertyMethod = getPropertyMethodDefinition.MakeGenericMethod(parameterInfo.ParameterType);
        return Expression.Convert(
            Expression.Call(messageVariable, getPropertyMethod, Expression.Constant(messagePropertyName))
            , parameterInfo.ParameterType);
    }

    private static Expression BuildGetServiceExpression(ParameterExpression serviceProviderParameter, CommonReflection reflection, ParameterInfo parameterInfo)
    {
        return Expression.Convert(Expression.Call(serviceProviderParameter
                , reflection.getServiceMethod
                , Expression.Constant(parameterInfo.ParameterType))
            , parameterInfo.ParameterType);
    }
}