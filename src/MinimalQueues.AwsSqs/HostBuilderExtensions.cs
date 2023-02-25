using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AwsSqs;

public static class HostBuilderExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsSqsListener(this IHostBuilder hostBuilder
        , string? queueUrl = null
        , AWSCredentials? credentials = null
        , RegionEndpoint? region = null
        , int maxConcurrency = 1
        , Func<int, TimeSpan>? backoffFunction = null
        , int waitTimeSeconds = 4
        , int visibilityTimeout = 30
        , int renewVisibilityTime = 24
        , int prefetchCount = 0
        , int requestMaxNumberOfMessages = 10
        , Action<Exception> onError = null)
    {
        var queueProcessorOptions = hostBuilder.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAwsSqsListener(queueUrl
            , credentials
            , region
            , maxConcurrency
            , backoffFunction
            , waitTimeSeconds
            , visibilityTimeout
            , renewVisibilityTime
            , prefetchCount
            , requestMaxNumberOfMessages
            , onError);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsSqsListener<TDependency>(this IHostBuilder hostBuilder
        , Action<AwsSqsConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        var queueProcessorOptions = hostBuilder.AddQueueProcessorHostedService();
        queueProcessorOptions.ConfigureAwsSqsListener();//to set defaults
        return queueProcessorOptions.ConfigureAwsSqsListener(configureConnection);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsSqsListener(this IHostBuilder hostBuilder
        , Action<AwsSqsConnectionConfiguration, IServiceProvider> configureConnection)
    {
        var queueProcessorOptions = hostBuilder.AddQueueProcessorHostedService();
        queueProcessorOptions.ConfigureAwsSqsListener();//to set defaults
        return queueProcessorOptions.ConfigureAwsSqsListener<IServiceProvider>(configureConnection);
    }
}