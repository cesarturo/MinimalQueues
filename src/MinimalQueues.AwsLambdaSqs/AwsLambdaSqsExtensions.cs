using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AwsLambdaSqs;

public static class AwsLambdaSqsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsLambdaSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , string? queueName = null
        , Action<Exception>? onError = null)
    {
        builder.ConfigureServices(services =>
        {
            services.TryAddSingleton<LambdaEventListenerHostedService>();
            services.AddHostedService(sp => sp.GetRequiredService<LambdaEventListenerHostedService>());
        });
        return builder.Configure((QueueProcessorOptions queueProcessorOptions, LambdaEventListenerHostedService lambdaListener) =>
        {
            var connection = new AwsLambdaSqsConnection { OnError = onError };
            lambdaListener.AddConnection(queueName, connection);
            queueProcessorOptions.Connection = connection;
        });
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddAwsLambdaSqsListener(this IHostBuilder hostBuilder
        , string? queueName = null
        , Action<Exception>? onError = null)
    {
        return hostBuilder.AddQueueProcessorHostedService()
                          .ConfigureAwsLambdaSqsListener(queueName, onError);
    }
}
