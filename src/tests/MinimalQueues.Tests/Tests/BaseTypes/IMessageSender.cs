public interface IMessageSender : IDisposable
{
    List<string> SentMessages { get; }
    Task PurgeQueueAsync();
    Task SendMessagesAsync(int count, TimeSpan executionTimeHeader);
}