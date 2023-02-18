using System.Runtime.InteropServices;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Hosting;

namespace MinimalQueues.AwsLambdaSqs;

internal sealed class MessageProcessor
{
    private readonly IHostApplicationLifetime _appLifetime;
    private Dictionary<string, AwsLambdaSqsConnection> NamedConnections = new();
    private AwsLambdaSqsConnection? DefaultConnection;

    public MessageProcessor(IHostApplicationLifetime appLifetime)
    {
        _appLifetime = appLifetime;
    }

    public void AddConnection(AwsLambdaSqsConnection connection)
    {
        if (connection.QueueArn is null)
        {
            DefaultConnection = DefaultConnection is null
                ? connection
                : throw new Exception("A default AwsLambdaSqsConnection is already registered.");
        }
        else if (!NamedConnections.TryAdd(connection.QueueArn, connection))
        {
            throw new Exception($"An AwsLambdaSqsConnection with arn {connection.QueueArn} is already registered.");
        }
    }

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var failedIds = await ProcessMessages(sqsEvent);
        return BuildResponse(failedIds);
    }

    private static SQSBatchResponse BuildResponse(string?[] failedIds)
    {
        var failures = failedIds.Where(failedId => failedId is not null)
            .Select(failedId => new SQSBatchResponse.BatchItemFailure { ItemIdentifier = failedId })
            .ToList();
        return new SQSBatchResponse { BatchItemFailures = failures };
    }

    private Task<string?[]> ProcessMessages(SQSEvent sqsEvent)
    {
        var records = CollectionsMarshal.AsSpan(sqsEvent.Records);
        var tasks = new Task<string?>[records.Length];
        for (int i = 0; i < records.Length; i++)
        {
            var record = records[i];
            tasks[i] = Task.Run(() => ProcessMessage(record));
        }
        return Task.WhenAll(tasks);
    }

    private async Task<string?> ProcessMessage(SQSEvent.SQSMessage record)
    {
        var connection = NamedConnections.TryGetValue(record.EventSourceArn, out var namedConnection)
            ? namedConnection
            : DefaultConnection;
        if (connection is null) return null;
        try
        {
            await connection.ProcessMessage(new LambdaSqsMessage(record), _appLifetime.ApplicationStopping);
        }
        catch
        {
            return record.MessageId;
        }

        return null;
    }
}