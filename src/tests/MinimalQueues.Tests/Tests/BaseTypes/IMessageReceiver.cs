using System.Collections.Concurrent;

public interface IMessageReceiver : IDisposable
{
    ConcurrentBag<string> ProcessedMessages { get; }
    Task StartAsync();
}