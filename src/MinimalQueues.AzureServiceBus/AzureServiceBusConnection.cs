using Azure.Core;
using Azure.Messaging.ServiceBus;
using MinimalQueues.Core;

namespace MinimalQueues.AzureServiceBus;

public sealed class AzureServiceBusConnection : IQueueConnection, IAzureServiceBusConnectionConfiguration, IAsyncDisposable
{
    public string? ConnectionString { get; set; }
    public string? Namespace { get; set; }
    public string? EntityPath { get; set; }
    public TokenCredential? Credential { get; set; }
    public ServiceBusClientOptions? ServiceBusClientOptions { get; set; }
    public ServiceBusProcessorOptions? ServiceBusProcessorOptions { get; set; }
    public Func<ProcessErrorEventArgs, Task>? ProcessError { get; set; }

    public Func<IMessage, CancellationToken, Task> ProcessMessageDelegate { private get; set; }
    private ServiceBusClient?    _serviceBusClient;
    private ServiceBusProcessor? _processor;

    public async Task Start(CancellationToken cancellationToken)
    {
        _serviceBusClient = GetClient();
        //AutoComplete must always be true:
        if (ServiceBusProcessorOptions is not null) ServiceBusProcessorOptions.AutoCompleteMessages = true;
        _processor = _serviceBusClient.CreateProcessor(EntityPath, ServiceBusProcessorOptions);
        _processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessError?? (_ => Task.CompletedTask);
        await _processor.StartProcessingAsync(cancellationToken);
    }
    private ServiceBusClient GetClient()
    {
        if (_serviceBusClient is not null)
            return _serviceBusClient;

        if (ConnectionString is not null)
        {
            EntityPath ??= ServiceBusConnectionStringProperties.Parse(ConnectionString).EntityPath;
            return new ServiceBusClient(ConnectionString, ServiceBusClientOptions);
        }

        if (   Namespace     is not null
            && EntityPath    is not null
            && Credential    is not null)
            return new ServiceBusClient(Namespace, Credential, ServiceBusClientOptions);

        if (Namespace is not null)
            return new ServiceBusClient(Namespace);

        throw new Exception("Missing Azure Connection Parameters");
    }

    private Task Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        return ProcessMessageDelegate!(new AzureMessage(arg.Message), arg.CancellationToken);
    }

    public async Task Stop()
    {
        if (_processor != null)
        {
            await _processor.DisposeAsync();
            _processor = null;
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (_processor != null) 
            await _processor.DisposeAsync();

        if (_serviceBusClient != null)
            await _serviceBusClient.DisposeAsync();
    }
}

public interface IAzureServiceBusConnectionConfiguration
{
    string? ConnectionString { get; set; }
    string? Namespace { get; set; }
    string? EntityPath { get; set; }
    TokenCredential? Credential { get; set; }
    ServiceBusClientOptions? ServiceBusClientOptions { get; set; }
    ServiceBusProcessorOptions? ServiceBusProcessorOptions { get; set; }
    Func<ProcessErrorEventArgs, Task>? ProcessError { get; set; }
}