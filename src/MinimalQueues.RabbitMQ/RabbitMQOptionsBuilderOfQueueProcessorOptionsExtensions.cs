using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.RabbitMQ;

public static class RabbitMQOptionsBuilderOfQueueProcessorOptionsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureRabbitMQListener(
        this IOptionsBuilder<QueueProcessorOptions> builder, Action<IRabbitMQConnectionConfiguration> configureConnection)
    {
        return builder.Configure(options =>
        {
            var connection = new RabbitMQConnection();
            configureConnection(connection);
            options.Connection = connection;
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureRabbitMQListener<TDependency>(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IRabbitMQConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        return builder.Configure<TDependency>((options, dependency) =>
        {
            var connection = new RabbitMQConnection();
            configureConnection(connection, dependency);
            options.Connection = connection;
        });
    }

    public static IOptionsBuilder<QueueProcessorOptions> ConfigureRabbitMQListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IRabbitMQConnectionConfiguration, IServiceProvider> configureConnection)
    {
        return builder.ConfigureRabbitMQListener<IServiceProvider>(configureConnection);
    }
}