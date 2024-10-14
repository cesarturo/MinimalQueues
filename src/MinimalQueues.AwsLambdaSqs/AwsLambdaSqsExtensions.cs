using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AwsLambdaSqs;

[Obsolete]
public static class AwsLambdaSqsExtensions
{
    [Obsolete]
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
    [Obsolete]
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsLambdaSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsLambdaSqsConnectionConfiguration, IServiceProvider> configure)
    {
        return builder.ConfigureAwsLambdaSqsListener<IServiceProvider>(configure);
    }
    [Obsolete]
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsLambdaSqsListener<TDependency>(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsLambdaSqsConnectionConfiguration, TDependency> configure) where TDependency : class
    {
        builder.ConfigureServices(services =>
        {
            services.TryAddSingleton<LambdaSqsEventHandler>();
            services.TryAddSingleton<LambdaBootstrapHostedService>();
            services.AddHostedService(sp => sp.GetRequiredService<LambdaBootstrapHostedService>());
        });
        return builder.Configure((QueueProcessorOptions queueProcessorOptions, TDependency dependency, LambdaSqsEventHandler messageProcessor) =>
        {
            var connection = new AwsLambdaSqsConnection();
            configure(connection, dependency);
            messageProcessor.AddConnection(connection);
            queueProcessorOptions.Connection = connection;
        });
    }
    [Obsolete]
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsLambdaSqsListener(this IHostBuilder hostBuilder
        , string? queueArn = null
        , Action<Exception>? onError = null)
    {
        return hostBuilder.AddQueueProcessorHostedService()
                          .ConfigureAwsLambdaSqsListener(queueArn, onError);
    }
    [Obsolete]
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsLambdaSqsListener(this IHostBuilder hostBuilder
        , Action<IAwsLambdaSqsConnectionConfiguration, IServiceProvider> configure)
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureAwsLambdaSqsListener(configure);
    }
    [Obsolete]
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsLambdaSqsListener<TDependency>(this IHostBuilder hostBuilder
        , Action<IAwsLambdaSqsConnectionConfiguration, TDependency> configure) where TDependency : class
    {
        return hostBuilder.AddQueueProcessorHostedService()
            .ConfigureAwsLambdaSqsListener(configure);
    }
}