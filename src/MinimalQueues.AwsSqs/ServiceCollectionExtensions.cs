﻿using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.AwsSqs.Connection;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AwsSqs;

public static class ServiceCollectionExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsSqsListener(this IServiceCollection services
        , string queueUrl
        , AWSCredentials? credentials = null
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
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsSqsListener<TDependency>(this IServiceCollection services
        , Action<AwsSqsConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        var queueProcessorOptions = services.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAwsSqsListener(configureConnection);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddAwsSqsListener(this IServiceCollection services
        , Action<AwsSqsConnectionConfiguration, IServiceProvider> configureConnection)
    {
        var queueProcessorOptions = services.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAwsSqsListener<IServiceProvider>(configureConnection);
    }
}