using Azure.Messaging.ServiceBus;
using MinimalQueues.Core;

namespace MinimalQueues.AzureServiceBus
{
    public sealed class AzureMessage : IMessage
    {
        private ServiceBusReceivedMessage _message;
        public AzureMessage(ServiceBusReceivedMessage message) => _message = message;

        public ServiceBusReceivedMessage InternalMessage => _message;

        public BinaryData GetBody() => _message.Body;
        public object? GetProperty(string propertyName)
        {
            _message.ApplicationProperties.TryGetValue(propertyName, out var value);
            return value;
        }
        public T? GetProperty<T>(string propertyName)
        {
            return _message.ApplicationProperties.TryGetValue(propertyName, out var value)
                ? (T)value
                : default;
        }
    }
}
