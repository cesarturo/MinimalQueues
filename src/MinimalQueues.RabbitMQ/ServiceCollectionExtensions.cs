using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.RabbitMQ;

public static class ServiceCollectionExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddRabbitMQListener(this IServiceCollection services
        , Action<IRabbitMQConnectionConfiguration> configureConnection)
    {
        return services.AddQueueProcessorHostedService()
                       .ConfigureRabbitMQListener(configureConnection);
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddRabbitMQListener<TDependency>(this IServiceCollection services
        , Action<IRabbitMQConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        return services.AddQueueProcessorHostedService()
            .ConfigureRabbitMQListener(configureConnection);
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddRabbitMQListener(this IServiceCollection services
        , Action<IRabbitMQConnectionConfiguration, IServiceProvider> configureConnection)
    {
        return services.AddQueueProcessorHostedService()
            .ConfigureRabbitMQListener(configureConnection);
    }
}