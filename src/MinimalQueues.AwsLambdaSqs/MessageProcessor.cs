using System.Runtime.InteropServices;
using System.Text.Json;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Hosting;

namespace MinimalQueues.AwsLambdaSqs;

internal sealed class MessageProcessor
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Dictionary<string, AwsLambdaSqsConnection> NamedConnections = new();
    private AwsLambdaSqsConnection? DefaultConnection;
    private readonly JsonSerializerOptions _inputSerializerOptions = new (){ PropertyNameCaseInsensitive = true };

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
    public async Task<Stream?> FunctionHandler(Stream stream, ILambdaContext context)
    {
        var sqsEvent = JsonSerializer.Deserialize<SQSEvent>(stream, _inputSerializerOptions);
        var messageIdOfErrors = await ProcessMessages(sqsEvent);
        return ResponseStreamBuilder.Build(messageIdOfErrors);
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
    {//When processing fails return the MessageId, when succeeds return null
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