using Amazon.SQS.Model;
using System.Diagnostics;

namespace MinimalQueues.AwsSqs
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
                try
                {
                    await using var messageContext = await _messageReceiver.ReceiveMessage(_connection.Cancellation);
                    if (messageContext is null) continue;
                    var message = messageContext.Message!;
                    using var activity = TryStartActivity(message);
                    await _connection.ProcessMessageAsync(messageContext.Message);
                    await _connection.DeleteMessage(messageContext.Message);
                    activity?.Stop();
                }
                catch (Exception exception)
                {
                    _connection.OnError?.Invoke(exception);
                }
            }
        }

        public Activity? TryStartActivity(Message message)
        {
            if (message.Attributes.TryGetValue("Diagnostic-Id", out string parentId))
            {
                var activity = new Activity("AwsSqs");
                activity.SetParentId(parentId);
                activity.Start();
                return activity;
            }
            return null;
        }
    }
}
