namespace MinimalQueues;

public interface IDeserializer
{
    T? Deserialize<T>(BinaryData data);
}