using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace MinimalQueues.AzureServiceBus;

public static class AzureOptionsBuilderOfQueueProcessorOptionsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAzureServiceBusListener(
        this IOptionsBuilder<QueueProcessorOptions> builder
        , string @namespace
        , string? entityPath = null
        , TokenCredential? credential = null
        , ServiceBusClientOptions? serviceBusClientOptions = null
        , ServiceBusProcessorOptions? serviceBusProcessorOptions = null
        , Func<ProcessErrorEventArgs, Task> onError = null)
    {
        return builder.Configure(options =>
        {
            var connection = EnsureConnectionInstance(options);
            connection.ConnectionString           = null;
            connection.Credential                 = credential ?? new DefaultAzureCredential();
            connection.EntityPath                 = entityPath;
            connection.Namespace                  = @namespace;
            connection.ServiceBusClientOptions    = serviceBusClientOptions;
            connection.ServiceBusProcessorOptions = serviceBusProcessorOptions;
            connection.ProcessError               = onError;
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAzureServiceBusListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , string connectionString
        , string entityPath
        , ServiceBusClientOptions? serviceBusClientOptions = null
        , ServiceBusProcessorOptions? serviceBusProcessorOptions = null
        , Func<ProcessErrorEventArgs, Task> onError = null)
    {
        return builder.Configure(options =>
        {
            var connection = EnsureConnectionInstance(options);
            connection.ConnectionString           = connectionString;
            connection.Credential                 = null;
            connection.EntityPath                 = entityPath;
            connection.Namespace                  = null;
            connection.ServiceBusClientOptions    = serviceBusClientOptions;
            connection.ServiceBusProcessorOptions = serviceBusProcessorOptions;
            connection.ProcessError               = onError;
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAzureServiceBusListener<TDependency>(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAzureServiceBusConnectionConfiguration, TDependency> configureConnection) where TDependency : class
    {
        return builder.Configure<TDependency>((options, dependency) =>
        {
            var connection = EnsureConnectionInstance(options);
            configureConnection(connection, dependency);
        });
    }

    public static IOptionsBuilder<QueueProcessorOptions> ConfigureAzureServiceBusListener(this IOptionsBuilder<QueueProcessorOptions> builder
        , Action<IAzureServiceBusConnectionConfiguration, IServiceProvider> configureConnection)
    {
        return builder.ConfigureAzureServiceBusListener<IServiceProvider>(configureConnection);
    }

    private static AzureServiceBusConnection EnsureConnectionInstance(QueueProcessorOptions options)
    {
        if (options.Connection is not null)
            return (AzureServiceBusConnection) options.Connection;

        var connection = new AzureServiceBusConnection();
        options.Connection = connection;
        return connection;

    }
}