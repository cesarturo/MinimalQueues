using MinimalSqsClient;

namespace MinimalQueues.AwsSqs;

internal sealed class OnDemandMessageReceiver : IMessageReceiver
{
    private readonly TimeSpan         _visibilityTimeout;
    private readonly ISqsClient       _sqsClient;
    private readonly AwsSqsConnection _connection;
    private readonly BackoffManager   _backoffManager;

    public OnDemandMessageReceiver(AwsSqsConnection connection)
    {
        var configuration = connection.Configuration;
        _connection        = connection;
        _visibilityTimeout = TimeSpan.FromSeconds(configuration.VisibilityTimeout);
        _sqsClient         = connection._sqsClient;
        _backoffManager    = new BackoffManager(connection);
    }
    
    public async Task<SqsMessage?> ReceiveMessage(CancellationToken cancellation)
    {
        await _backoffManager.Wait();
        var renewTimer = new PeriodicTimer(TimeSpan.FromSeconds(_connection.Configuration.RenewVisibilityWaitTime));
        try
        {
            var countdown = new Countdown(_visibilityTimeout);
            countdown.Start();
            MinimalSqsClient.SqsMessage? message = await ReceiveOneMessage(cancellation);
            if (message is null)
            {
                _backoffManager.Start();
                renewTimer.Dispose();
                return null;
            }
            _backoffManager.Release();
            if (countdown.TimedOut)
            {
                throw new Exception($"{nameof(_sqsClient.ReceiveMessageAsync)} took longer than {nameof(AwsSqsConnectionConfiguration.VisibilityTimeout)}. Message lock may be expired. Message will not be processed.");
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
    private async Task<MinimalSqsClient.SqsMessage?> ReceiveOneMessage(CancellationToken cancellation)
    {
        try
        {
            var sqsMessage = await _sqsClient.ReceiveMessageAsync(_connection.Configuration.WaitTimeSeconds, _connection.Configuration.VisibilityTimeout);
            return sqsMessage;
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