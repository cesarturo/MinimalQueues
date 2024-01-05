using System.Text.Json;
using MinimalSqsClient;

public class SqsMessageSender : IMessageSender
{
    public List<string> SentMessages { get; } = new();
    private SqsClient _sqsClient;
    private int i;

    public SqsMessageSender(string queueUrl)
    {
        _sqsClient = new SqsClient(new SqsClientOptions
        {
            QueueUrl = queueUrl //Environment.GetEnvironmentVariable("QUEUE_URL")!//TestContext.Parameters["QueueUrl"]
        });
    }

    public async Task PurgeQueueAsync()
    {
        await _sqsClient.PurgeQueueAsync();
        await Task.Delay(400);//Aws docs recommend to wait for 60 secs, I don't want to (https://docs.aws.amazon.com/AWSSimpleQueueService/latest/APIReference/API_PurgeQueue.html)
    }

    public async Task SendMessagesAsync(int count, TimeSpan? executionTimeHeader)
    {
        var batches = Enumerable.Range(0, count).GroupBy(i => i % 10, i => GenerateMessage());

        var headers = new Dictionary<string, string?>();
        
        if (executionTimeHeader.HasValue)
            headers["execution-time"] = executionTimeHeader.ToString();

        foreach (var batch in batches)
        {
            var batchToSend = batch.Select(message => JsonSerializer.Serialize(message)).ToArray();
            await _sqsClient.SendMessageBatchAsync(batchToSend, headers);
            SentMessages.AddRange(batch);
        }

        string GenerateMessage()
        {
            Interlocked.Increment(ref i);
            return $"testmessage-{i}";
        }
    }

    public void Dispose()
    {
        _sqsClient.Dispose();
        SentMessages.Clear();
    }
}