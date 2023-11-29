using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;

public interface IMessageReceiver : IDisposable
{
    ConcurrentBag<string> ProcessedMessages { get; }
    Task StartAsync(Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> configureListener);
}