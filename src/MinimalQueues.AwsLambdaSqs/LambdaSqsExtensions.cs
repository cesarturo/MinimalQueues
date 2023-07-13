using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AwsLambdaSqs;

public static class LambdaSqsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureLambdaBootstrapListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , string? queueArn = null
        , Action<Exception>? onError = null)
    {
        return builder.ConfigureLambdaBootstrapListener((configuration, _) =>
        {
            configuration.QueueArn = queueArn;
            configuration.OnError = onError;
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureLambdaBootstrapListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsLambdaSqsConnectionConfiguration, IServiceProvider> configure)
    {
        return builder.ConfigureLambdaBootstrapListener<IServiceProvider>(configure);
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureLambdaBootstrapListener<TDependency>(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsLambdaSqsConnectionConfiguration, TDependency> configure) where TDependency : class
    {
        builder.ConfigureServices(services => services.TryAddSingleton<MessageProcessor>(_ => new MessageProcessor()));

        return builder.Configure((QueueProcessorOptions queueProcessorOptions, TDependency dependency, MessageProcessor messageProcessor) =>
        {
            var connection = new AwsLambdaSqsConnection();
            configure(connection, dependency);
            messageProcessor.AddConnection(connection);
            queueProcessorOptions.Connection = connection;
        });
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddLambdaBootstrapListener(this IHostBuilder hostBuilder
        , string? queueArn = null
        , Action<Exception>? onError = null)
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureLambdaBootstrapListener(queueArn, onError);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddLambdaBootstrapListener(this IHostBuilder hostBuilder
        , Action<IAwsLambdaSqsConnectionConfiguration, IServiceProvider> configure)
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureLambdaBootstrapListener(configure);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddLambdaBootstrapListener<TDependency>(this IHostBuilder hostBuilder
        , Action<IAwsLambdaSqsConnectionConfiguration, TDependency> configure) where TDependency : class
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureLambdaBootstrapListener(configure);
    }
}



