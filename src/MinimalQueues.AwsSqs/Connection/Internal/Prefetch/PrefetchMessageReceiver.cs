using System.Threading.Channels;
using MinimalSqsClient;

namespace MinimalQueues.AwsSqs.Connection.Internal.Prefetch;

internal class PrefetchMessageReceiver : IMessageReceiver
{
    private readonly AwsSqsConnection _connection;
    private readonly ISqsClient _sqsClient;
    private readonly ChannelReader<SqsMessage> _channelReader;
    private readonly ChannelWriter<SqsMessage> _channelWriter;
    private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(true);
    private int _backoffCount;
    private readonly Task _prefetchTask;
    private CancellationToken _connectionCancellation;
    public PrefetchMessageReceiver(AwsSqsConnection connection)
    {
        _connection = connection;
        _connectionCancellation = connection.Cancellation;
        _sqsClient = connection._sqsClient;
        var channel = Channel.CreateBounded<SqsMessage>(new BoundedChannelOptions(_connection.Configuration.PrefetchCount)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = false,
        });
        _channelReader = channel.Reader;
        _channelWriter = channel.Writer;
        _prefetchTask = Prefetch();
    }

    private async Task Prefetch()
    {
        try
        {
            int prefetchCount = _connection.Configuration.PrefetchCount;
            while (!_connectionCancellation.IsCancellationRequested)
            {
                var countForRequest = await GetCountForRequestGreaterThanTheMinimum(prefetchCount);
                if (_connectionCancellation.IsCancellationRequested) break;
                var messages = await GetPrefetchedMessagesOrNull(countForRequest, prefetchCount);
                if (messages is null)
                {
                    await BackoffWait();
                    continue;
                }
                foreach (var message in messages)
                {
                    await _channelWriter.WriteAsync(message);
                }
                _backoffCount = 0;
            }
        }
        catch (ChannelClosedException exception)
        {
            
        }
    }
    private async Task<int> GetCountForRequestGreaterThanTheMinimum(int prefetchCount)
    {
        const int minRequestCount = 1;
        int countForRequest;
        while (!_connectionCancellation.IsCancellationRequested)
        {
            countForRequest = prefetchCount - _channelReader.Count;
            if (countForRequest >= minRequestCount)
            {
                return Math.Min(countForRequest, _connection.Configuration.RequestMaxNumberOfMessages);
            }
            await _autoResetEvent.WaitAsync(_connection.Cancellation);
        }
        return -1;
    }
    private async Task<IEnumerable<MessageGroup.PrefetchedSqsMessage>?> GetPrefetchedMessagesOrNull(int countForRequest, int prefetchCount)
    {
        var messageGroup = new MessageGroup(_connection);
        var messages = await ReceiveMessages(countForRequest);
        if (messages is null or { Count: 0 })
        {
            await messageGroup.DisposeAsync();
            return null;
        }
        return messageGroup.Initialize(messages);
    }

    private async Task BackoffWait()
    {
        var backoffTime = _connection.Configuration.BackOffFunction(++_backoffCount);
        using var timer = new PeriodicTimer(backoffTime);
        await timer.WaitForNextTickAsync(_connectionCancellation);
    }

    private async Task<List<MinimalSqsClient.SqsMessage>?> ReceiveMessages(int countForRequest)
    {
        var config = _connection.Configuration;
        while (true)
        {
            try
            {
                var countdown = new Countdown(TimeSpan.FromSeconds(config.VisibilityTimeout));
                countdown.Start();
                var response = await _sqsClient.ReceiveMessagesAsync(countForRequest, config.WaitTimeSeconds, config.VisibilityTimeout);
                if (countdown.TimedOut)
                {
                    var exception = new Exception($"{nameof(_sqsClient.ReceiveMessageAsync)} took longer than {nameof(AwsSqsConnectionConfiguration.VisibilityTimeout)}. Message locks may be expired. Messages will not be processed.");
                    config.OnError?.Invoke(exception);
                }
                return response;
            }
            catch (Exception exception)
            {
                config.OnError?.Invoke(exception);
                return null;
            }
        }
    }

    public async Task<SqsMessage?> ReceiveMessage(CancellationToken cancellation)
    {
        var messageContext = await _channelReader.ReadAsync();
        _autoResetEvent.Set();
        return messageContext;
    }

    public void Dispose()
    {
        _autoResetEvent.Dispose();
        _channelWriter.Complete();
    }
}