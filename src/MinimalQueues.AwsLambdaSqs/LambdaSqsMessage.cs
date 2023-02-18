using MinimalQueues.Core;

namespace MinimalQueues.AwsLambdaSqs;

public sealed class LambdaSqsMessage : IMessage
{
    public LambdaSqsMessage(SQSEvent.SQSMessage message)
    {
        InternalMessage = message;
        _binaryData = new BinaryData(message.Body);
    }

    public SQSEvent.SQSMessage InternalMessage { get; }
    private BinaryData  _binaryData;
    public object? GetProperty(string propertyName)
    {
        return InternalMessage.MessageAttributes.TryGetValue(propertyName, out var attr) 
            ? attr.StringValue 
            : default;
    }

    public T? GetProperty<T>(string propertyName)
    {
        return InternalMessage.MessageAttributes.TryGetValue(propertyName, out var attr)
            ? (T?)Convert.ChangeType(attr.StringValue, typeof(T))
            : default;
    }

    public BinaryData GetBody() => _binaryData;
}