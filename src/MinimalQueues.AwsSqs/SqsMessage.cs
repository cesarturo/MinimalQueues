using MinimalQueues.Core;

namespace MinimalQueues.AwsSqs;

public abstract class SqsMessage : IMessage, IAsyncDisposable
{
    public MinimalSqsClient.SqsMessage InternalMessage { get; }

    protected SqsMessage(MinimalSqsClient.SqsMessage internalMessage)
    {
        InternalMessage = internalMessage;
    }
    public object? GetProperty(string propertyName)
    {
        InternalMessage.MessageAttributes.TryGetValue(propertyName, out var value);
        return value;
    }
    public T? GetProperty<T>(string propertyName)
    {
        if (InternalMessage.MessageAttributes.TryGetValue(propertyName, out var value))
        {
            if (value is T stringValue) return stringValue;
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return default;
    }

    public abstract BinaryData GetBody();
    public abstract ValueTask DisposeAsync();
}