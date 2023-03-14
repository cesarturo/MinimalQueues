using System.Diagnostics.CodeAnalysis;
using MinimalQueues.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MinimalQueues.RabbitMQ;

internal sealed class RabbitMQConnection : IQueueConnection, IRabbitMQConnectionConfiguration, IDisposable
{
    #pragma warning disable CS8618
    private Func<IMessage, CancellationToken, Task> _processMessageAsync;
    #pragma warning restore CS8618
    private IConnection? _connection;
    private IModel? _channel;
    public string? QueueName { get; set; }
    public Action<ConnectionFactory>? ConfigureConnectionFactory { get; set; }
    public Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken)
    {
        Validate();
        _processMessageAsync = processMessageDelegate;
        var connectionFactory = new ConnectionFactory();
        ConfigureConnectionFactory(connectionFactory);
        connectionFactory.DispatchConsumersAsync = true;

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        
        var consumer = new AsyncEventingBasicConsumer(_channel);

        async Task OnConsumerOnReceived(object ch, BasicDeliverEventArgs ea)
        {
            try
            {
                await _processMessageAsync(new RabbitMQMessage(ea), cancellationToken);
            }
            catch (Exception)
            {
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
            _channel.BasicAck(ea.DeliveryTag, false);
        }

        consumer.Received += OnConsumerOnReceived;
        
        _channel.BasicConsume(QueueName, false, consumer);
        return Task.CompletedTask;
    }
    [MemberNotNull(nameof(QueueName), nameof(ConfigureConnectionFactory))]
    private void Validate()
    {
        if (QueueName is null)
            throw new Exception($"{nameof(QueueName)} was not provided.");

        if (ConfigureConnectionFactory is null)
            throw new Exception($"{ConfigureConnectionFactory} delegate was not provided.");
    }

    public void Dispose()
    {
        if (_channel != null) 
            ExecuteIgnoringException(_channel.Close);
        if (_connection != null)
        {
            ExecuteIgnoringException(_connection.Close);
            ExecuteIgnoringException(_connection.Dispose);
        }

        void ExecuteIgnoringException(Action action)
        {
            try { action(); } catch { }
        }
    }
}
//https://stackoverflow.com/questions/53374144/rabbitmq-ack-timeout
//https://www.rabbitmq.com/consumers.html#acknowledgement-timeout


public interface IRabbitMQConnectionConfiguration
{
    public string? QueueName { get; set; }
    public Action<ConnectionFactory>? ConfigureConnectionFactory { get; set; }
}