using Azure.Core;
using Azure.Messaging.ServiceBus;

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

    #pragma warning disable CS8618
    private Func<IMessage, CancellationToken, Task> _processMessageAsync;
    #pragma warning restore CS8618
    private ServiceBusClient?    _serviceBusClient;
    private ServiceBusProcessor? _processor;
    public async Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken)
    {
        _processMessageAsync = processMessageDelegate;
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
        if (ConnectionString is not null)
            return new ServiceBusClient(ConnectionString, ServiceBusClientOptions); 
        if(   Namespace     is not null
           && EntityPath    is not null
           && Credential    is not null)
            return new ServiceBusClient(Namespace, Credential, ServiceBusClientOptions);
        if (Namespace is not null)
            return new ServiceBusClient(Namespace);
        throw new Exception("Missing Azure Connection Parameters");
    }

    private Task Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        return _processMessageAsync(new AzureMessage(arg.Message), arg.CancellationToken);
    }

    public async Task Stop()
    {
        if (_processor != null)
        {
            await _processor.CloseAsync();
            _processor = null;
        }
        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync();
            _serviceBusClient = null;
        }
    }
    public async ValueTask DisposeAsync()
    {
        await Stop();
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