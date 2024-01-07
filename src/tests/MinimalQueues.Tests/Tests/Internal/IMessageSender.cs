namespace Tests.Internal;

public interface IMessageSender : IDisposable
{
    List<string> SentMessages { get; }
    Task<bool> PurgeQueueAsync();
    Task SendMessagesAsync(int count, TimeSpan? executionTimeHeader = null);
}