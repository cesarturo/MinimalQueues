using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using MinimalQueues.Options;

namespace MinimalQueues.AzureServiceBus;

public static class HostBuilderExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener(this IHostBuilder hostBuilder
        , string @namespace
        , string entityPath
        , TokenCredential? credential = null
        , ServiceBusClientOptions? serviceBusClientOptions = null
        , ServiceBusProcessorOptions? serviceBusProcessorOptions = null
        , Func<ProcessErrorEventArgs, Task>? onError = null)
    {
        var queueProcessorOptions = hostBuilder.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener(@namespace, entityPath, credential, serviceBusClientOptions, serviceBusProcessorOptions, onError);
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener(this IHostBuilder hostBuilder
        , string connectionString
        , string entityPath
        , ServiceBusClientOptions? serviceBusClientOptions = null
        , ServiceBusProcessorOptions? serviceBusProcessorOptions = null
        , Func<ProcessErrorEventArgs, Task>? onError = null)
    {
        var queueProcessorOptions = hostBuilder.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener(connectionString, entityPath, serviceBusClientOptions, serviceBusProcessorOptions, onError);
    }

    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener(this IHostBuilder hostBuilder
        , Action<IAzureServiceBusConnectionConfiguration, IServiceProvider> configureConnection)
    {
        var queueProcessorOptions = hostBuilder.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener(configureConnection);
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener<TDependency>(this IHostBuilder hostBuilder
        , Action<IAzureServiceBusConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        var queueProcessorOptions = hostBuilder.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener<TDependency>(configureConnection);
    }
}