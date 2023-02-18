using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AwsLambdaSqs;

public static class AwsLambdaSqsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsLambdaSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , string? queueArn = null
        , Action<Exception>? onError = null)
    {
        return builder.ConfigureAwsLambdaSqsListener((configuration, _) =>
        {
            configuration.QueueArn = queueArn;
            configuration.OnError = onError;
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsLambdaSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsLambdaSqsConnectionConfiguration, IServiceProvider> configure)
    {
        return builder.ConfigureAwsLambdaSqsListener<IServiceProvider>(configure);
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsLambdaSqsListener<TDependency>(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsLambdaSqsConnectionConfiguration, TDependency> configure) where TDependency : class
    {
        builder.ConfigureServices(services =>
        {
            services.TryAddSingleton<MessageProcessor>();
            services.TryAddSingleton<LambdaEventListenerHostedService>();
            services.AddHostedService(sp => sp.GetRequiredService<LambdaEventListenerHostedService>());
        });
        return builder.Configure((QueueProcessorOptions queueProcessorOptions, TDependency dependency, MessageProcessor messageProcessor) =>
        {
            var connection = new AwsLambdaSqsConnection();
            configure(connection, dependency);
            messageProcessor.AddConnection(connection);
            queueProcessorOptions.Connection = connection;
        });
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddAwsLambdaSqsListener(this IHostBuilder hostBuilder
        , string? queueArn = null
        , Action<Exception>? onError = null)
    {
        return hostBuilder.AddQueueProcessorHostedService()
                          .ConfigureAwsLambdaSqsListener(queueArn, onError);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsLambdaSqsListener(this IHostBuilder hostBuilder
        , Action<IAwsLambdaSqsConnectionConfiguration, IServiceProvider> configure)
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureAwsLambdaSqsListener(configure);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsLambdaSqsListener<TDependency>(this IHostBuilder hostBuilder
        , Action<IAwsLambdaSqsConnectionConfiguration, TDependency> configure) where TDependency : class
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureAwsLambdaSqsListener(configure);
    }
}
