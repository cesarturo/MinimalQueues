using System.Threading.Channels;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace MinimalQueues.AwsSqs;

internal class PrefetchMessageReceiver : IMessageReceiver
{
    private readonly AwsSqsConnection _connection;
    private readonly AmazonSQSClient _sqsClient;
    private readonly ChannelReader<MessageGroup.MessageContext> _channelReader;
    private readonly ChannelWriter<MessageGroup.MessageContext> _channelWriter;
    private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(true);
    private readonly ReceiveMessageRequest _receiveMessageRequest;
    private int _backoffCount;
    private readonly Task _prefetchTask;
    private CancellationToken _connectionCancellation;
    public PrefetchMessageReceiver(AwsSqsConnection connection)
    {
        _connection = connection;
        _connectionCancellation = connection.Cancellation;
        _sqsClient = connection._sqsClient;
        var allAttributes = new List<string>(new[] { "All" });
        _receiveMessageRequest = new ReceiveMessageRequest(_connection.QueueUrl)
        {
            WaitTimeSeconds = _connection.WaitTimeSeconds,
            AttributeNames = allAttributes,
            MessageAttributeNames = allAttributes,
            VisibilityTimeout = _connection.VisibilityTimeout
        };
        var channel = Channel.CreateBounded<MessageGroup.MessageContext>(new BoundedChannelOptions(_connection.PrefetchCount)
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
        int prefetchCount = _connection.PrefetchCount;
        while (!_connectionCancellation.IsCancellationRequested)
        {
            var countForRequest = await GetCountForRequestGreaterThanTheMinimum(prefetchCount);
            if (_connectionCancellation.IsCancellationRequested) break;
            var messageContexts = await WaitUntilAvailableAndGetMessageContexts(countForRequest, prefetchCount);
            foreach (var messageContext in messageContexts)
            {
                await _channelWriter.WriteAsync(messageContext);
            }
            _backoffCount = 0;
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
                return Math.Min(countForRequest, _connection.RequestMaxNumberOfMessages);
            }
            await _autoResetEvent.WaitAsync(_connection.Cancellation);
        }
        return -1;
    }
    private async Task<IEnumerable<MessageGroup.MessageContext>> WaitUntilAvailableAndGetMessageContexts(int countForRequest, int prefetchCount)
    {
        while (true)
        {
            var messageGroup = new MessageGroup(_connection);
            var messages = await ReceiveMessages(countForRequest);
            if (messages.Count is 0)
            {
                await messageGroup.DisposeAsync();
                await BackoffWait();
                countForRequest = prefetchCount - _channelReader.Count;
                countForRequest = Math.Min(countForRequest, _connection.RequestMaxNumberOfMessages);
                continue;
            }
            return messageGroup.Initialize(messages);
        }
    }

    private async Task BackoffWait()
    {
        var backoffTime = _connection.BackOffFunction(++_backoffCount);
        using var timer = new PeriodicTimer(backoffTime);
        await timer.WaitForNextTickAsync(_connectionCancellation);
    }

    private async Task<List<Message>> ReceiveMessages(int countForRequest)
    {
        while (true)
        {
            _receiveMessageRequest.MaxNumberOfMessages = countForRequest;
            var countdown = new Countdown(TimeSpan.FromSeconds(_connection.VisibilityTimeout));
            countdown.Start();
            var response = await _sqsClient.ReceiveMessageAsync(_receiveMessageRequest, _connectionCancellation);
            if (countdown.TimedOut)
            {
                var exception = new Exception($"{nameof(_sqsClient.ReceiveMessageAsync)} took longer than {nameof(AwsSqsConnection.VisibilityTimeout)}. Message locks may be expired. Messages will not be processed.");
                _connection.OnError?.Invoke(exception);
            }
            return response.Messages; 
        }
    }

    public async Task<IMessageContext?> ReceiveMessage(CancellationToken cancellation)
    {
        var messageContext = await _channelReader.ReadAsync();
        _autoResetEvent.Set();
        return messageContext;
    }
}