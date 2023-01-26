using Amazon.SQS.Model;

namespace MinimalQueues.AwsSqs;

internal sealed class SqsMessage : IMessage
{
    private Message _message;
    public SqsMessage(Message message) => _message = message;
    public BinaryData GetBody() => new BinaryData(_message.Body);
    public object? GetProperty(string propertyName)
    {
        _message.MessageAttributes.TryGetValue(propertyName, out var value);
        return value?.StringValue;
    }
    public T? GetProperty<T>(string propertyName)
    {
        if (_message.MessageAttributes.TryGetValue(propertyName, out var value))
        {
            if (value.StringValue is T stringValue) return stringValue;
            return (T)Convert.ChangeType(value.StringValue, typeof(T));
        }
        return default;
    }
}