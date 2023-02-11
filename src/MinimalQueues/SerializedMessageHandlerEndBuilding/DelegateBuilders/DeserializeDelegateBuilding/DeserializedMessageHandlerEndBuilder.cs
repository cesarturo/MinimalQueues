namespace MinimalQueues;

internal static class DeserializedMessageHandlerEndBuilder
{
    public static DeserializedMessageHandlerEnd<T> Build<T>(EndOptions endOptions, HandlerOptions handlerOptions)
    {
        var instance = new DeserializedMessageHandlerEnd<T>();
        Func<IServiceProvider, BinaryData, T?> deserializeFn;
        deserializeFn = GetDeserializeFunction<T>(endOptions, handlerOptions);
        instance.Deserialize = deserializeFn;
        return instance;
    }

    private static Func<IServiceProvider, BinaryData, T?> GetDeserializeFunction<T>(EndOptions endOptions, HandlerOptions handlerOptions)
    {
        if (endOptions.DeserializerInstance is { } endDeserializer)
        {
            return GetDeserializeFunction<T>(endDeserializer);
        }
        if (endOptions.DeserializerType is { } endDeserializerType)
        {
            return GetDeserializeFunction<T>(endDeserializerType);
        }
        if (handlerOptions.DeserializerInstance is { } handlerDeserializer)
        {
            return GetDeserializeFunction<T>(handlerDeserializer);
        }
        if (handlerOptions.DeserializerType is { } handlerDeserializerType)
        {
            return GetDeserializeFunction<T>(handlerDeserializerType);
        }
        return DeserializerWithServiceProvider.Deserialize<IDeserializer, T>;
    }

    private static Func<IServiceProvider, BinaryData, T?> GetDeserializeFunction<T>(Type deserializerType)
    {
        var methodInfo = typeof(DeserializerWithServiceProvider)
            .GetMethod(nameof(DeserializerWithServiceProvider.Deserialize))!
            .MakeGenericMethod(typeof(T), deserializerType);
        return (Func<IServiceProvider, BinaryData, T>)
            Delegate.CreateDelegate(typeof(Func<IServiceProvider, BinaryData, T>), methodInfo);
    }

    private static Func<IServiceProvider, BinaryData, T?> GetDeserializeFunction<T>(IDeserializer deserializer)
    {
        var deserializerWithInstance = new DeserializerWithInstance(deserializer);
        return deserializerWithInstance.Deserialize<T>;
    }
}