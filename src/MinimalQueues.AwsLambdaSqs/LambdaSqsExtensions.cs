using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AwsLambdaSqs;

public static class LambdaSqsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureLambdaSqsHandler(this IOptionsBuilder<QueueProcessorOptions> builder
        , string? queueArn = null
        , Action<Exception>? onError = null)
    {
        return builder.ConfigureLambdaSqsHandler((configuration, _) =>
        {
            configuration.QueueArn = queueArn;
            configuration.OnError = onError;
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureLambdaSqsHandler(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsLambdaSqsConnectionConfiguration, IServiceProvider> configure)
    {
        return builder.ConfigureLambdaSqsHandler<IServiceProvider>(configure);
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureLambdaSqsHandler<TDependency>(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsLambdaSqsConnectionConfiguration, TDependency> configure) where TDependency : class
    {
        builder.ConfigureServices(services =>
        {
            services.TryAddSingleton<LambdaSqsEventProcessor>(_ => new LambdaSqsEventProcessor());
            services.TryAddSingleton<LambdaSqsEventHandler>(sp => sp.GetRequiredService<LambdaSqsEventProcessor>().FunctionHandler);
        });

        return builder.Configure((QueueProcessorOptions queueProcessorOptions, TDependency dependency, LambdaSqsEventProcessor messageProcessor) =>
        {
            var connection = new AwsLambdaSqsConnection();
            configure(connection, dependency);
            messageProcessor.AddConnection(connection);
            queueProcessorOptions.Connection = connection;
        });
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddLambdaSqsMessageHandler(this IHostBuilder hostBuilder
        , string? queueArn = null
        , Action<Exception>? onError = null)
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureLambdaSqsHandler(queueArn, onError);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddLambdaSqsMessageHandler(this IHostBuilder hostBuilder
        , Action<IAwsLambdaSqsConnectionConfiguration, IServiceProvider> configure)
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureLambdaSqsHandler(configure);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddLambdaSqsMessageHandler<TDependency>(this IHostBuilder hostBuilder
        , Action<IAwsLambdaSqsConnectionConfiguration, TDependency> configure) where TDependency : class
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureLambdaSqsHandler(configure);
    }
}



