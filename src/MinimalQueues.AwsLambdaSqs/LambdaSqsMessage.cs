using Amazon.Lambda.SQSEvents;
using MinimalQueues.Core;

namespace MinimalQueues.AwsLambdaSqs;

public sealed class LambdaSqsMessage : IMessage
{
    public LambdaSqsMessage(SQSEvent.SQSMessage message)
    {
        InternalMessage = message;
    }

    public SQSEvent.SQSMessage InternalMessage { get; set; }
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

    public BinaryData GetBody()
    {
        return new BinaryData(InternalMessage.Body);
    }
}