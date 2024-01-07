using System.Text.Json;
using MinimalSqsClient;
using Polly;
using Polly.Retry;
using Tests.Internal;

namespace Tests.Implementations.AwsSqs;

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

    public async Task<bool> PurgeQueueAsync()
    {
        var retryPipeline = CreateRetryPipeline();

        var success = await retryPipeline.ExecuteAsync(async _ => await _sqsClient.PurgeQueueAsync());

        await Task.Delay(2000);//Aws docs recommend to wait for 60 secs, I don't want to (https://docs.aws.amazon.com/AWSSimpleQueueService/latest/APIReference/API_PurgeQueue.html)

        return success;

        static ResiliencePipeline<bool> CreateRetryPipeline()
        {
            return new ResiliencePipelineBuilder<bool>().AddRetry(new RetryStrategyOptions<bool>
            {
                ShouldHandle = new PredicateBuilder<bool>().HandleResult(false).Handle<Exception>(),
                MaxRetryAttempts = 5,
                BackoffType = DelayBackoffType.Linear,
                Delay = TimeSpan.FromSeconds(3)
            }).Build();
        }
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