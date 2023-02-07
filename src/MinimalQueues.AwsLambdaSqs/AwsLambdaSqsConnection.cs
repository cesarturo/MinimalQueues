using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using MinimalQueues.Core;

namespace MinimalQueues.AwsLambdaSqs;

public class AwsLambdaSqsConnection: IQueueConnection, IAsyncDisposable
{
    public Action<Exception>? OnError { get; set; }

    private Task? _bootstrapTask;
    private Func<IMessage, CancellationToken, Task>? _processMessageDelegate;
    private CancellationTokenSource _cancellation;
    internal CancellationToken Cancellation => _cancellation.Token;
    public Task Start(Func<IMessage, CancellationToken, Task> processMessageDelegate, CancellationToken cancellationToken)
    {
        _cancellation = new CancellationTokenSource();
        _processMessageDelegate = processMessageDelegate;
        _bootstrapTask = LambdaBootstrapBuilder
            .Create<SQSEvent>(FunctionHandler, new DefaultLambdaJsonSerializer())
            .Build()
            .RunAsync(Cancellation);
        return Task.CompletedTask;
    }
    private async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var tasks = sqsEvent.Records
            .Select(record => new LambdaSqsMessage(record))
            .Select(message => Task.Run(() => ProcessMessage(message), Cancellation));
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (TaskCanceledException exception)
        {

        }
    }

    private async Task ProcessMessage(LambdaSqsMessage message)
    {
        try
        {
            await _processMessageDelegate!(message, Cancellation);
        }
        catch (Exception exception)
        {
            OnError?.Invoke(exception);
        }
    }
    public async ValueTask DisposeAsync()
    {
        _cancellation.Cancel();
        if (_bootstrapTask != null)
        {
            await _bootstrapTask;
            _bootstrapTask.Dispose();
        }
    }
}