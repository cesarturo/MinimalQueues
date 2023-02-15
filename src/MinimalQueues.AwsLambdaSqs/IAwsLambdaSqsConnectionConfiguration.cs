namespace MinimalQueues.AwsLambdaSqs;

public interface IAwsLambdaSqsConnectionConfiguration
{
    string? QueueArn { get; set; }
    Action<Exception>? OnError { get; set; }
}