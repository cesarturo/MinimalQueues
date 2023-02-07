using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AwsLambdaSqs;

public static class AwsLambdaSqsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsLambdaSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<Exception>? onError = null)
    {
        return builder.Configure(queueProcessorOptions =>
        {
            queueProcessorOptions.Connection = new AwsLambdaSqsConnection
            {
                OnError = onError
            };
        });
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddAwsLambdaSqsListener(this IHostBuilder hostBuilder
        , Action<Exception>? onError = null)
    {
        return hostBuilder.AddQueueProcessorHostedService()
                          .ConfigureAwsLambdaSqsListener(onError);
    }
}