using System.Collections.Concurrent;

namespace Tests.Internal;

public interface IMessageReceiver : IDisposable
{
    ConcurrentBag<string> ProcessedMessages { get; }
    Task StartAsync();
}