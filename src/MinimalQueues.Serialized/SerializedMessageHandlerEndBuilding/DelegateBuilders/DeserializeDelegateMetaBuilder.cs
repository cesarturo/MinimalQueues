using System.Linq.Expressions;

namespace MinimalQueues.Deserialization
{
    internal static class DeserializeDelegateMetaBuilder
    {
        internal static dynamic Build(EndDelegateParameters endDelegateParameters, EndOptions options,
            HandlerOptions handlerOptions)
        {
            var reflection = new CommonReflection();
            var deserializeMethod = reflection.DeserializeMethodGenericDefinition
                .MakeGenericMethod(endDelegateParameters.BodyParameter!.ParameterType);

            var serviceProviderParameter = Expression.Parameter(reflection.serviceProviderType);
            var binaryDataParameter = Expression.Parameter(reflection.BinaryDataType);

            var serializerInstanceExpression = GetDeserializerInstanceExpression(options, handlerOptions, serviceProviderParameter, reflection);

            var body = Expression.Call(serializerInstanceExpression, deserializeMethod, binaryDataParameter);

            dynamic builtDelegate = Expression.Lambda(body, serviceProviderParameter, binaryDataParameter).Compile();

            return builtDelegate;
        }

        private static Expression GetDeserializerInstanceExpression(EndOptions endOptions
            , HandlerOptions handlerOptions
            , ParameterExpression serviceProviderParameter
            , CommonReflection reflection)
        {
            if (endOptions.DeserializerInstance is { } endDeserializer)
            {
                return Expression.Constant(endDeserializer);
            }
            if (endOptions.DeserializerType is Type endDeserializerType)
            {
                return Expression.Convert(
                    Expression.Call(serviceProviderParameter, reflection.getServiceMethod, Expression.Constant(endDeserializerType))
                    , endDeserializerType);
            }
            if (handlerOptions.DeserializerInstance is {} handlerDeserializer)
            {
                return Expression.Constant(handlerDeserializer);
            }
            if (handlerOptions.DeserializerType is Type handlerDeserializerType)
            {
                return Expression.Convert(
                    Expression.Call(serviceProviderParameter, reflection.getServiceMethod, Expression.Constant(handlerDeserializerType))
                    , handlerDeserializerType);
            }
            return Expression.Convert(
                Expression.Call(serviceProviderParameter, reflection.getServiceMethod, Expression.Constant(typeof(IDeserializer)))
                , typeof(IDeserializer));
        }
    }
}