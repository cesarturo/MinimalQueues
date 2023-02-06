namespace MinimalQueues.Core;

public interface IMessage : IMessageProperties
{
    BinaryData GetBody();
}
public interface IMessageProperties
{
    object? GetProperty(string propertyName);
    T? GetProperty<T>(string propertyName);
}