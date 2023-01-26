using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.Options;

namespace MinimalQueues.AwsSqs;

public static class ServiceCollectionExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsSqsListener(this ServiceCollection services
        , string queueUrl
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
        var queueProcessorOptions = services.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAwsSqsListener(
            queueUrl
            , credentials
            , clientConfig
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
}