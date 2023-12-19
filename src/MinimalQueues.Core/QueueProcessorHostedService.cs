using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MinimalQueues.Core;

public sealed class QueueProcessorHostedService : IHostedService, IAsyncDisposable
{
    private readonly IServiceScopeFactory     _scopeFactory;
    private readonly IHostApplicationLifetime _lifeTime;
    private readonly QueueProcessorOptions    _options;

    public QueueProcessorHostedService(QueueProcessorOptions options
                                     , IServiceScopeFactory scopeFactory
                                     , IHostApplicationLifetime lifeTime)
    {
        _scopeFactory = scopeFactory;
        _lifeTime     = lifeTime;
        _options      = options;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Connection is null) throw new InvalidOperationException("A connection has not been configured.");
        _options.Connection.ProcessMessageDelegate = Processor_ProcessMessageAsync;
        await _options.Connection.Start(_lifeTime.ApplicationStopping);
    }
    private async Task Processor_ProcessMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        await new MessageHandlerDelegatesInvoker(_options.MessageHandlerDelegates, _lifeTime.ApplicationStopping)
            .Handle(message, scope.ServiceProvider);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_options.Connection != null)
            await _options.Connection.Stop();
    }

    public async ValueTask DisposeAsync()
    {
        switch (_options.Connection)
        {
            case IAsyncDisposable asyncDisposable: await asyncDisposable.DisposeAsync(); break;
            case IDisposable disposable: disposable.Dispose(); break;
        }
    }
}