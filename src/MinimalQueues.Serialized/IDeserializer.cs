namespace MinimalQueues.Deserialization;

public interface IDeserializer
{
    T? Deserialize<T>(BinaryData data);
}