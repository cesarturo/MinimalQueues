using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MinimalQueues;

public class QueueProcessorHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly QueueProcessorOptions _options;
    private Func<IMessage, IServiceProvider, Task> _handlerDelegate;

    public QueueProcessorHostedService(QueueProcessorOptions options, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _options = options;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Connection is null) throw new InvalidOperationException("A connection has not been configured.");
        _handlerDelegate = HandlerDelegateBuilder.Build(_options.MessageHandlerDelegates, cancellationToken);
        await _options.Connection.Start(Processor_ProcessMessageAsync, cancellationToken);
    }
    private async Task Processor_ProcessMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        await _handlerDelegate(message, serviceProvider);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_options.Connection is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
            return;
        }
        if(_options.Connection is IDisposable disposable)
            disposable.Dispose();
    }
}