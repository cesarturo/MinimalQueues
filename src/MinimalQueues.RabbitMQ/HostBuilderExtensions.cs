using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.RabbitMQ;

public static class HostBuilderExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddRabbitMQListener(this IHostBuilder hostbuilder
        , Action<IRabbitMQConnectionConfiguration> configureConnection)
    {
        return hostbuilder.AddQueueProcessorHostedService()
            .ConfigureRabbitMQListener(configureConnection);
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddRabbitMQListener<TDependency>(this IHostBuilder hostbuilder
        , Action<IRabbitMQConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        return hostbuilder.AddQueueProcessorHostedService()
            .ConfigureRabbitMQListener(configureConnection);
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddRabbitMQListener(this IHostBuilder hostbuilder
        , Action<IRabbitMQConnectionConfiguration, IServiceProvider> configureConnection)
    {
        return hostbuilder.AddQueueProcessorHostedService()
            .ConfigureRabbitMQListener(configureConnection);
    }
}