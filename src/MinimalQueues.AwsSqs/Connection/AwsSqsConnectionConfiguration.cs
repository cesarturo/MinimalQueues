using Amazon;
using Amazon.Runtime;

namespace MinimalQueues.AwsSqs.Connection;

public sealed class AwsSqsConnectionConfiguration
{
    public RegionEndpoint? Region { get; set; }
    public AWSCredentials? Credentials { get; set; }
    public string? QueueUrl { get; set; }
    public int WaitTimeSeconds { get; set; } = 4;
    public int VisibilityTimeout { get; set; } = 30;
    public int RenewVisibilityWaitTime { get; set; } = 24;

    public int MaxConcurrency { get; set; } = 1;
    public int PrefetchCount { get; set; } = 0;
    public int RequestMaxNumberOfMessages { get; set; } = 10;
    public Func<int, TimeSpan>? BackOffFunction { get; set; }
    public Action<Exception>? OnError { get; set; }

}