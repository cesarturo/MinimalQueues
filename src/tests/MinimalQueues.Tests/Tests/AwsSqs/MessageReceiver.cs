using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalQueues;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

public class MessageReceiver : IDisposable
{
    private IHost? _host;
    public ConcurrentBag<string> ProcessedMessages { get; } = new();
    private readonly Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> _configureListener;

    public MessageReceiver(Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> configureListener)
    {
        _configureListener = configureListener;
    }
    public async Task StartAsync()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddSingleton(ProcessedMessages));

        var queueApp = _configureListener(hostBuilder)
            .UseDeserializedMessageHandler();

        queueApp.Use(async (string message, ConcurrentBag<string> receivedMessages, [Prop("execution-time")] string executionTime) =>
        {
            await Task.Delay(TimeSpan.Parse(executionTime));
            receivedMessages.Add(message);
        });

        _host = hostBuilder.Build();

        await _host.StartAsync();
    }

    public void Dispose()
    {
        _host?.Dispose();

        ProcessedMessages.Clear();
    }
}