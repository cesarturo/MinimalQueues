using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalQueues;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

public class SqsMessageReceiver : IMessageReceiver
{
    private IHost _host;
    public ConcurrentBag<string> ProcessedMessages { get; } = new();

    
    public async Task StartAsync(Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> configureListener)
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddSingleton(ProcessedMessages));

        var queueApp = configureListener(hostBuilder)
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
        _host.Dispose();
        ProcessedMessages.Clear();
    }
}