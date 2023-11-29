using System.Linq.Expressions;
using System.Reflection;
using MinimalQueues.Core;

namespace MinimalQueues;

internal ref struct HandleAsyncDelegateMetaBuilder
{
    private readonly EndDelegateMetadata _endDelegateMetadata;
    private readonly EndpointOptions     _options;

    private ParameterExpression? _serviceProviderParameter;
    private ParameterExpression? _deserializedMessageParameter;
    private ParameterExpression? _cancellationTokenParameter;
    private ParameterExpression? _messagePropertiesParameter;
    private CommonReflection?    _reflection;

    internal static Func<IServiceProvider, IMessageProperties, CancellationToken, Task> Build(
        EndDelegateMetadata endDelegateMetadata, EndpointOptions options) 
            => new HandleAsyncDelegateMetaBuilder(endDelegateMetadata, options).Build();

    internal static Func<IServiceProvider, TMessage?, IMessageProperties, CancellationToken, Task> Build<TMessage>(
        EndDelegateMetadata endDelegateMetadata, EndpointOptions options) 
            => new HandleAsyncDelegateMetaBuilder(endDelegateMetadata, options).Build<TMessage>();

    private HandleAsyncDelegateMetaBuilder(EndDelegateMetadata endDelegateMetadata, EndpointOptions options)
    {
        _endDelegateMetadata = endDelegateMetadata;
        _options = options;
    }
    
    private Func<IServiceProvider, IMessageProperties, CancellationToken, Task> Build()
    {
        var lambdaParameters = CreateParameterExpressions(includeMessageParameter: false);

        var bodyExpression = CreateBodyExpression();

        return Expression.Lambda<Func<IServiceProvider, IMessageProperties, CancellationToken, Task>>(
                            bodyExpression, lambdaParameters)
                         .Compile();
    }

    private Func<IServiceProvider, TMessage?, IMessageProperties, CancellationToken, Task> Build<TMessage>()
    {
        var lambdaParameters = CreateParameterExpressions(includeMessageParameter: true);

        var bodyExpression = CreateBodyExpression();

        return Expression.Lambda<Func<IServiceProvider, TMessage?, IMessageProperties, CancellationToken, Task>>(
                            bodyExpression, lambdaParameters)
                         .Compile();
    }

    private List<ParameterExpression> CreateParameterExpressions(bool includeMessageParameter)
    {
        _serviceProviderParameter     = Expression.Parameter(typeof(IServiceProvider));
        _messagePropertiesParameter   = Expression.Parameter(typeof(IMessageProperties));
        _deserializedMessageParameter = includeMessageParameter
            ? Expression.Parameter(_endDelegateMetadata.BodyParameter!.ParameterType)
            : null;
        _cancellationTokenParameter   = Expression.Parameter(typeof(CancellationToken));

        var lambdaParameters = new List<ParameterExpression>(includeMessageParameter ? 4 : 3);
        lambdaParameters.Add(_serviceProviderParameter);
        if (includeMessageParameter) lambdaParameters.Add(_deserializedMessageParameter!);
        lambdaParameters.Add(_messagePropertiesParameter);
        lambdaParameters.Add(_cancellationTokenParameter);
        return lambdaParameters;
    }

    private MethodCallExpression CreateBodyExpression()
    {
        var targetInstanceExpression = _options.HandlerDelegate.Target is { } delegateTarget
            ? Expression.Constant(delegateTarget)
            : null;

        _reflection = new CommonReflection();

        var invokationArguments = CreateArgumentExpressions(_endDelegateMetadata.ParametersClassified);

        return Expression.Call(targetInstanceExpression
                             , _options.HandlerDelegate.Method
                             , invokationArguments);
    }

    private Expression[] CreateArgumentExpressions(EndDelegateParameterInfo[] parametersMetadata)
    {
        var argumentExpressions = new Expression[parametersMetadata.Length];

        for(var i = 0; i < parametersMetadata.Length; i++) 
            argumentExpressions[i] = CreateArgumentExpression(parametersMetadata[i]);

        return argumentExpressions;
    }

    private Expression CreateArgumentExpression(EndDelegateParameterInfo parameterMetadata)
    {
        var parameterInfo = parameterMetadata.ParameterInfo;

        return parameterMetadata.Type switch
        {
            ParameterKind.service      => BuildGetServiceExpression(_serviceProviderParameter, _reflection, parameterInfo),
            ParameterKind.prop         => BuildGetPropertyExpression(_messagePropertiesParameter, _reflection.GetPropertyMethodDefinition, parameterInfo),
            ParameterKind.body         => _deserializedMessageParameter!,
            ParameterKind.cancellation => _cancellationTokenParameter,
            _ => throw new Exception("Unknown Parameter type")
        };
    }

    private static Expression BuildGetPropertyExpression(ParameterExpression messageVariable
                                                        ,MethodInfo getPropertyMethodDefinition
                                                        ,ParameterInfo parameterInfo)
    {
        var propAttribute = parameterInfo.GetCustomAttribute<PropAttribute>()!;
        var messagePropertyName = propAttribute.PropertyName;
        var getPropertyMethod = getPropertyMethodDefinition.MakeGenericMethod(parameterInfo.ParameterType);
        return Expression.Convert(
            Expression.Call(messageVariable, getPropertyMethod, Expression.Constant(messagePropertyName))
            , parameterInfo.ParameterType);
    }

    private static Expression BuildGetServiceExpression(ParameterExpression serviceProviderParameter
                                                      , CommonReflection reflection
                                                      , ParameterInfo parameterInfo)
    {
        return Expression.Convert(Expression.Call(serviceProviderParameter
                , reflection.getServiceMethod
                , Expression.Constant(parameterInfo.ParameterType))
            , parameterInfo.ParameterType);
    }
}
