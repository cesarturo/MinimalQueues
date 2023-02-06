using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.AzureServiceBus;
using MinimalQueues.Core.Options;

namespace MinimalQueues.Core.AzureServiceBus;

public static class ServiceCollectionExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddAzureServiceBusListener(this ServiceCollection services
        , string @namespace
        , string entityPath
        , TokenCredential? credential = null
        , ServiceBusClientOptions? serviceBusClientOptions = null
        , ServiceBusProcessorOptions? serviceBusProcessorOptions = null
        , Func<ProcessErrorEventArgs, Task>? onError = null)
    {
        var queueProcessorOptions = services.AddQueueProcessorHostedService();
        return queueProcessorOptions.ConfigureAzureServiceBusListener(@namespace, entityPath, credential, serviceBusClientOptions, serviceBusProcessorOptions, onError);
    }
}