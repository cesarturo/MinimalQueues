namespace MinimalQueues;

internal sealed class DeserializerWithInstance
{
    private readonly IDeserializer _deserializer;

    public DeserializerWithInstance(IDeserializer deserializer)
    {
        _deserializer = deserializer;
    }
    public T? Deserialize<T>(IServiceProvider _, BinaryData data)
    {
        return _deserializer.Deserialize<T>(data);
    }
}