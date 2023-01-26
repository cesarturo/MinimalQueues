using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using MinimalQueues.Options;

namespace MinimalQueues.AwsSqs;

public static class OptionsBuilderOfQueueProcessorOptionsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , string? queueUrl = null
        , AWSCredentials? credentials = null
        , AmazonSQSConfig? clientConfig = null
        , RegionEndpoint? region = null
        , int maxConcurrency = 1
        , Func<int, TimeSpan>? backoffFunction = null
        , int waitTimeSeconds = 4
        , int visibilityTimeout = 30
        , int renewVisibilityTime = 24
        , int prefetchCount = 0
        , int requestMaxNumberOfMessages = 10
        , Action<Exception>? onError = null)
    {
        return builder.Configure(queueProcessorOptions =>
        {
            var connection                        = EnsureConnectionInstance(queueProcessorOptions);
            connection.QueueUrl                   = queueUrl;
            connection.Credentials                = credentials;
            connection.ClientConfig               = clientConfig;
            connection.Region                     = region;
            connection.MaxConcurrentCalls         = maxConcurrency;
            connection.BackOffFunction            = backoffFunction ?? (i => TimeSpan.FromSeconds(2));
            connection.WaitTimeSeconds            = waitTimeSeconds;
            connection.VisibilityTimeout          = visibilityTimeout;
            connection.RenewVisibilityWaitTime    = renewVisibilityTime;
            connection.PrefetchCount              = prefetchCount;
            connection.RequestMaxNumberOfMessages = requestMaxNumberOfMessages;
            connection.OnError                    = onError;
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsSqsListener<TDependency>(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsSqsConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        return builder.Configure<TDependency>((options, injected) =>
        {
            var connection = EnsureConnectionInstance(options);
            configureConnection(connection, injected);
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAwsSqsConnectionConfiguration, IServiceProvider> configureConnection)
    {
        return builder.ConfigureAwsSqsListener<IServiceProvider>(configureConnection);
    }

    private static AwsSqsConnection EnsureConnectionInstance(QueueProcessorOptions queueProcessorOptions)
    {
        if (queueProcessorOptions.Connection is not null)
            return (AwsSqsConnection)queueProcessorOptions.Connection;

        var connection = new AwsSqsConnection();
        queueProcessorOptions.Connection = connection;
        return connection;
    }
}