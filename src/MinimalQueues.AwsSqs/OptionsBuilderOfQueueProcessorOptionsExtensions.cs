using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using MinimalQueues.AwsSqs.Connection;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;
using MinimalSqsClient;

namespace MinimalQueues.AwsSqs;

public static class OptionsBuilderOfQueueProcessorOptionsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , string? queueUrl                     = null
        , AWSCredentials? credentials          = null
        , RegionEndpoint? region               = null
        , int maxConcurrency                   = 1
        , Func<int, TimeSpan>? backoffFunction = null
        , int waitTimeSeconds                  = 4
        , int visibilityTimeout                = 30
        , int renewVisibilityTime              = 24
        , int prefetchCount                    = 0
        , int requestMaxNumberOfMessages       = 10
        , Action<Exception>? onError           = null)
    {
        return builder.ConfigureAwsSqsListener((config, _) =>
        {
            config.QueueUrl                   = queueUrl;
            config.Credentials                = credentials;
            config.Region                     = region;
            config.MaxConcurrency             = maxConcurrency;
            config.BackOffFunction            = backoffFunction;
            config.WaitTimeSeconds            = waitTimeSeconds;
            config.VisibilityTimeout          = visibilityTimeout;
            config.RenewVisibilityWaitTime    = renewVisibilityTime;
            config.PrefetchCount              = prefetchCount;
            config.RequestMaxNumberOfMessages = requestMaxNumberOfMessages;
            config.OnError                    = onError;
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsSqsListener<TDependency>(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<AwsSqsConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        builder.AddOptions<AwsSqsConnectionConfiguration>(builder.Name).Configure(configureConnection);
        builder.ConfigureServices(services =>
            services.AddSqsClient(builder.Name).Configure<IOptionsMonitor<AwsSqsConnectionConfiguration>>((sqsClientOptions, connectionOptions) =>
            {
                var options               = connectionOptions.Get(builder.Name);
                if (options.QueueUrl is null) throw new Exception("QueueUrl was not provided.");
                sqsClientOptions.QueueUrl = options.QueueUrl;
                sqsClientOptions.Region   = options.Region?.SystemName;
            }));
        return builder.Configure<ISqsClientFactory, IOptionsMonitor<AwsSqsConnectionConfiguration>>(
            (options, sqsClientFactory, connectionConfig) =>
            {
                options.Connection = new AwsSqsConnection(sqsClientFactory.Get(builder.Name), connectionConfig.Get(builder.Name));
            });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAwsSqsListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<AwsSqsConnectionConfiguration, IServiceProvider> configureConnection)
    {
        return builder.ConfigureAwsSqsListener<IServiceProvider>(configureConnection);
    }
}