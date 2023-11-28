namespace MinimalQueues;

internal static class DeserializeDelegateBuilder
{
    internal static Func<IServiceProvider, BinaryData, T?> Build<T>(EndOptions endOptions, HandlerOptions handlerOptions)
    {
        if (endOptions.DeserializerInstance is { } endDeserializer)
            return GetDeserializeFunction<T>(endDeserializer);

        if (endOptions.DeserializerType is { } endDeserializerType)
            return GetDeserializeFunction<T>(endDeserializerType);

        if (handlerOptions.DeserializerInstance is { } handlerDeserializer)
            return GetDeserializeFunction<T>(handlerDeserializer);

        if (handlerOptions.DeserializerType is { } handlerDeserializerType)
            return GetDeserializeFunction<T>(handlerDeserializerType);

        return FromServiceProviderDeserializer.Deserialize<IDeserializer, T>;
    }

    private static Func<IServiceProvider, BinaryData, T?> GetDeserializeFunction<T>(Type deserializerType)
    {
        var methodInfo = typeof(FromServiceProviderDeserializer)
            .GetMethod(nameof(FromServiceProviderDeserializer.Deserialize))!
            .MakeGenericMethod(typeof(T), deserializerType);

        return (Func<IServiceProvider, BinaryData, T>)
            Delegate.CreateDelegate(typeof(Func<IServiceProvider, BinaryData, T>), methodInfo);
    }

    private static Func<IServiceProvider, BinaryData, T?> GetDeserializeFunction<T>(IDeserializer deserializer)
    {
        var deserializerWithInstance = new FromInstanceDeserializer(deserializer);
        return deserializerWithInstance.Deserialize<T>;
    }
}