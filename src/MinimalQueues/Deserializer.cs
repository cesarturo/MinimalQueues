using System.Text.Json;

namespace MinimalQueues;

public class Deserializer : IDeserializer
{
    private readonly JsonSerializerOptions? _options;

    public Deserializer(JsonSerializerOptions? options = null)
    {
        _options = options;
    }

    public T? Deserialize<T>(BinaryData data)
    {
        return data.ToObjectFromJson<T>(_options);
    }
}