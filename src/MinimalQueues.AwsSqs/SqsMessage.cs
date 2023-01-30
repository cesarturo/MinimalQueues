using Amazon.SQS.Model;

namespace MinimalQueues.AwsSqs;

internal abstract class SqsMessage : IMessage, IAsyncDisposable
{
    public Message InnerMessage { get; }

    protected SqsMessage(Message innerMessage)
    {
        InnerMessage = innerMessage;
    }
    public object? GetProperty(string propertyName)
    {
        InnerMessage.MessageAttributes.TryGetValue(propertyName, out var value);
        return value?.StringValue;
    }
    public T? GetProperty<T>(string propertyName)
    {
        if (InnerMessage.MessageAttributes.TryGetValue(propertyName, out var value))
        {
            if (value.StringValue is T stringValue) return stringValue;
            return (T)Convert.ChangeType(value.StringValue, typeof(T));
        }
        return default;
    }

    public abstract BinaryData GetBody();
    public abstract ValueTask DisposeAsync();
}