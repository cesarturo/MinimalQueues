using System.Linq.Expressions;
using System.Reflection;
using MinimalQueues.Core;

namespace MinimalQueues;

internal static class MatchDelegateMetaBuilder
{
    internal static Func<IMessageProperties, bool> Build(Delegate matchDelegate)
    {
        var messagePropertiesParameter = Expression.Parameter(typeof(IMessageProperties));

        var getPropertyExpressions = GetArgumentExpressions(matchDelegate, messagePropertiesParameter);

        var targetInstanceExpression = matchDelegate.Target is { } delegateTarget
                                        ? Expression.Constant(delegateTarget)
                                        : null;

        return Expression.Lambda<Func<IMessageProperties, bool>>(
            Expression.Call(targetInstanceExpression, matchDelegate.Method, getPropertyExpressions)
            , messagePropertiesParameter).Compile();
    }

    private static List<MethodCallExpression> GetArgumentExpressions(Delegate matchDelegate, ParameterExpression messagePropertiesParameter)
    {
        var parameters = matchDelegate.Method.GetParameters();

        var getPropertyMethodDefinition = typeof(IMessageProperties).GetMethod(nameof(IMessageProperties.GetProperty)
            , 1
            , new[] { typeof(string) })!;

        var getPropertyExpressions = parameters.Select(parameterInfo =>
        {
            var propertyName = parameterInfo.GetCustomAttribute<PropAttribute>()?.PropertyName ??
                               parameterInfo.Name;
            var getPropertyMethod = getPropertyMethodDefinition.MakeGenericMethod(parameterInfo.ParameterType);

            return Expression.Call(messagePropertiesParameter, getPropertyMethod, Expression.Constant(propertyName));
        }).ToList();
        return getPropertyExpressions;
    }
}

