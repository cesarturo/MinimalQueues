using System.Text;
using MinimalQueues.Core;
using RabbitMQ.Client.Events;

namespace MinimalQueues.RabbitMQ;

public sealed class RabbitMQMessage: IMessage
{
    public BasicDeliverEventArgs InternalMessage { get; }

    private readonly BinaryData _body;

    public RabbitMQMessage(BasicDeliverEventArgs deliveryEvent)
    {
        InternalMessage = deliveryEvent;
        _body = new BinaryData(InternalMessage.Body);
    }
    public object? GetProperty(string propertyName)
    {
        return InternalMessage.BasicProperties.Headers.TryGetValue(propertyName, out var value)
            ? value : null;
    }

    public T? GetProperty<T>(string propertyName)
    {
        var headers = InternalMessage.BasicProperties.Headers;
        if (headers is null) return default;
        if (!headers.TryGetValue(propertyName, out var value)) 
            return default;

        if (typeof(T) == typeof(string))
        {
            return value is byte[] valueBytes
                ? (T)(object)Encoding.UTF8.GetString(valueBytes)
                : throw PropertyNotOfTypeException<T>(propertyName);
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            throw PropertyNotOfTypeException<T>(propertyName);
        }
    }

    private static Exception PropertyNotOfTypeException<T>(string propertyName)
    {
        return new Exception($"Property {propertyName} is not of type {typeof(T)}");
    }

    public BinaryData GetBody()
    {
        return _body;
    }
}