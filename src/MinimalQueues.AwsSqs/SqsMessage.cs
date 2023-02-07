using Amazon.SQS.Model;
using MinimalQueues.Core;

namespace MinimalQueues.AwsSqs;

public abstract class SqsMessage : IMessage, IAsyncDisposable
{
    public Message InternalMessage { get; }

    protected SqsMessage(Message internalMessage)
    {
        InternalMessage = internalMessage;
    }
    public object? GetProperty(string propertyName)
    {
        InternalMessage.MessageAttributes.TryGetValue(propertyName, out var value);
        return value?.StringValue;
    }
    public T? GetProperty<T>(string propertyName)
    {
        if (InternalMessage.MessageAttributes.TryGetValue(propertyName, out var value))
        {
            if (value.StringValue is T stringValue) return stringValue;
            return (T)Convert.ChangeType(value.StringValue, typeof(T));
        }
        return default;
    }

    public abstract BinaryData GetBody();
    public abstract ValueTask DisposeAsync();
}