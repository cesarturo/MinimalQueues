namespace MinimalQueues;

internal sealed class FromInstanceDeserializer
{
    private readonly IDeserializer _deserializer;

    public FromInstanceDeserializer(IDeserializer deserializer)
    {
        _deserializer = deserializer;
    }
    public T? Deserialize<T>(IServiceProvider _, BinaryData data)
    {
        return _deserializer.Deserialize<T>(data);
    }
}