using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.AzureServiceBus;
using MinimalQueues.Core.Options;

namespace MinimalQueues.Core.AzureServiceBus;

public static class ServiceCollectionExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener(this IServiceCollection services
        , string @namespace
        , string? entityPath = null
        , TokenCredential? credential = null
        , ServiceBusClientOptions? serviceBusClientOptions = null
        , ServiceBusProcessorOptions? serviceBusProcessorOptions = null
        , Func<ProcessErrorEventArgs, Task>? onError = null)
    {
        var queueProcessorOptions = services.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener(@namespace, entityPath, credential, serviceBusClientOptions, serviceBusProcessorOptions, onError);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener(this IServiceCollection services
        , string connectionString
        , string? entityPath = null
        , ServiceBusClientOptions? serviceBusClientOptions = null
        , ServiceBusProcessorOptions? serviceBusProcessorOptions = null
        , Func<ProcessErrorEventArgs, Task>? onError = null)
    {
        var queueProcessorOptions = services.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener(connectionString, entityPath, serviceBusClientOptions, serviceBusProcessorOptions, onError);
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener(this IServiceCollection services
        , Action<IAzureServiceBusConnectionConfiguration, IServiceProvider> configureConnection)
    {
        var queueProcessorOptions = services.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener(configureConnection);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener<TDependency>(this IServiceCollection services
        , Action<IAzureServiceBusConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        var queueProcessorOptions = services.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener<TDependency>(configureConnection);
    }
}