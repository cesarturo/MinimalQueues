using Amazon.SQS;
using Amazon.SQS.Model;
using System.Buffers;

namespace MinimalQueues.AwsSqs;

internal sealed class OnDemandMessageReceiver : IMessageReceiver
{
    private readonly TimeSpan         _visibilityTimeout;
    private readonly AmazonSQSClient  _sqsClient;
    private readonly AwsSqsConnection _connection;
    private readonly BackoffManager   _backoffManager;

    public OnDemandMessageReceiver(AwsSqsConnection connection)
    {
        _connection        = connection;
        _visibilityTimeout = TimeSpan.FromSeconds(connection.VisibilityTimeout);
        _sqsClient         = connection._sqsClient;
        _backoffManager    = new BackoffManager(connection);
        var allAttributes  = new List<string>(new[] { "All" });
        _receiveMessageRequest = new ReceiveMessageRequest(connection.QueueUrl)
        {
            MaxNumberOfMessages   = 1,
            WaitTimeSeconds       = connection.WaitTimeSeconds,
            AttributeNames        = allAttributes,
            MessageAttributeNames = allAttributes,
            VisibilityTimeout     = connection.VisibilityTimeout
        };
    }
    private ReceiveMessageRequest _receiveMessageRequest;
    
    public async Task<SqsMessage?> ReceiveMessage(CancellationToken cancellation)
    {
        await _backoffManager.Wait();
        var renewTimer = new PeriodicTimer(TimeSpan.FromSeconds(_connection.RenewVisibilityWaitTime));
        try
        {
            var countdown = new Countdown(_visibilityTimeout);
            countdown.Start();
            Message? message = await ReceiveOneMessage(cancellation);
            if (message is null)
            {
                _backoffManager.Start();
                renewTimer.Dispose();
                return null;
            }
            _backoffManager.Release();
            if (countdown.TimedOut)
            {
                throw new Exception($"{nameof(_sqsClient.ReceiveMessageAsync)} took longer than {nameof(AwsSqsConnection.VisibilityTimeout)}. Message lock may be expired. Message will not be processed.");
            }
            return new OnDemandSqsMessage(message, _connection, renewTimer);
        }
        catch (TaskCanceledException)
        {
            renewTimer.Dispose();
            return null;
        }
        catch
        {
            renewTimer.Dispose();
            throw;
        }
    }
    private async Task<Message?> ReceiveOneMessage(CancellationToken cancellation)
    {
        try
        {
            var messageResponse = await _sqsClient.ReceiveMessageAsync(_receiveMessageRequest, cancellation);
            var message = messageResponse.Messages.FirstOrDefault();
            return message;
        }
        catch
        {
            _backoffManager.Start();
            throw;
        }
    }

    public void Dispose()
    {
        
    }
}