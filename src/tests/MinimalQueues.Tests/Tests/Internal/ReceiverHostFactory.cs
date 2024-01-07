using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalQueues;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

namespace Tests.Internal;

public static class ReceiverHostFactory
{
    public static IHost Create(Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> configureListener)
    {
        ConcurrentBag<string> processedMessages = new();

        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders())
            .ConfigureServices(services => services.AddSingleton(processedMessages));
        
        var queueApp = configureListener(hostBuilder)
            .UseDeserializedMessageHandler();

        queueApp.Use(async (string message, ConcurrentBag<string> receivedMessages, [Prop("execution-time")] string? executionTime) =>
        {
            if (executionTime is not null)
                await Task.Delay(TimeSpan.Parse(executionTime));

            receivedMessages.Add(message);
        });

        return hostBuilder.Build();
    }

    public static IReadOnlyCollection<string> GetProcessedMessages(this IHost host)
        => host.Services.GetRequiredService<ConcurrentBag<string>>();

}