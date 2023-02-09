using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Hosting;

namespace MinimalQueues.AwsLambdaSqs;

internal sealed class LambdaEventListenerHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private Task? _bootstrapperTask;
    public LambdaEventListenerHostedService(IHostApplicationLifetime appLifetime)
    {
        _appLifetime = appLifetime;
    }
    private void StartLambda()
    {
        _bootstrapperTask = LambdaBootstrapBuilder
            .Create<SQSEvent>(FunctionHandler, new DefaultLambdaJsonSerializer())
            .Build()
            .RunAsync(_appLifetime.ApplicationStopping);
    }
    private Dictionary<string, AwsLambdaSqsConnection> NamedConnections = new();
    private AwsLambdaSqsConnection DefaultConnection;
    public void AddConnection(string? queueName, AwsLambdaSqsConnection connection)
    {
        if (queueName is null)
        {
            DefaultConnection = DefaultConnection is null 
                ? connection
                : throw new Exception("A default AwsLambdaSqsConnection is already registered.");
        }
        else if (!NamedConnections.TryAdd(queueName, connection))
        {
            throw new Exception($"An AwsLambdaSqsConnection named {queueName} is already registered.");
        }
    }

    private async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var tasks = sqsEvent.Records.Select(record => Task.Run(async () =>
        {
            var queueName = GetQueueName(record);
            var connection = NamedConnections.TryGetValue(queueName, out var namedConnection)
                            ? namedConnection
                            : DefaultConnection;
            await connection.ProcessMessage(new LambdaSqsMessage(record), _appLifetime.ApplicationStopping);
        }));
        await Task.WhenAll(tasks);
    }
    private static string GetQueueName(SQSEvent.SQSMessage record)
    {
        var arn = record.EventSourceArn;
        return arn.AsSpan(arn.LastIndexOf(':') + 1).ToString();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(StartLambda);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bootstrapperTask!;
    }
}