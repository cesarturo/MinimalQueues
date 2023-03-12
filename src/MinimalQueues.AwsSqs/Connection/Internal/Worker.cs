using System.Diagnostics;

namespace MinimalQueues.AwsSqs.Connection.Internal
{
    internal sealed class Worker
    {
        private readonly AwsSqsConnection _connection;
        private readonly IMessageReceiver _messageReceiver;

        public Worker(AwsSqsConnection connection, IMessageReceiver messageReceiver)
        {
            _connection = connection;
            _messageReceiver = messageReceiver;
        }
        public async Task Start()
        {
            while (!_connection.Cancellation.IsCancellationRequested)
            {
                SqsMessage? messageToAbandon = null;
                try
                {
                    await using var message = await _messageReceiver.ReceiveMessage(_connection.Cancellation);
                    if (message is null) continue;
                    messageToAbandon = message;
                    using var activity = TryStartActivity(message.InternalMessage);
                    await _connection.ProcessMessageAsync(message);
                    await _connection.DeleteMessageAsync(message);
                    activity?.Stop();
                }
                catch (Exception exception)
                {
                    await SafeAbandonMessage(messageToAbandon);
                    SafeInvokeOnError(exception);
                }
            }
        }
        public Activity? TryStartActivity(MinimalSqsClient.SqsMessage message)
        {
            if (message.MessageAttributes.TryGetValue("Diagnostic-Id", out string? parentId))
            {
                var activity = new Activity("AwsSqs");
                activity.SetParentId(parentId);
                activity.Start();
                return activity;
            }
            return null;
        }

        private async Task SafeAbandonMessage(SqsMessage? messageToAbandon)
        {
            if (messageToAbandon is null) return;
            try
            {
                await _connection.AbandonMessageAsync(messageToAbandon);
            }
            catch
            {
                // ignored
            }
        }

        private void SafeInvokeOnError(Exception exception)
        {
            try
            {
                _connection.Configuration.OnError?.Invoke(exception);
            }
            catch
            {
                // ignored
            }
        }

        
    }
}
