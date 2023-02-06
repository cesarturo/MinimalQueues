using Amazon.SQS.Model;
using System.Diagnostics;
using MinimalQueues.AwsSqs;

namespace MinimalQueues.Core.AwsSqs
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
                    await using var message = await _messageReceiver.ReceiveMessage(_connection.Cancellation);
                    if (message is null) continue;
                    using var activity = TryStartActivity(message.InnerMessage);
                    await _connection.ProcessMessageAsync(message);
                    await _connection.DeleteMessage(message);
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
