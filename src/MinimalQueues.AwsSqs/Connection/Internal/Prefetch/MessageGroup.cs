namespace MinimalQueues.AwsSqs.Connection.Internal.Prefetch;

internal class MessageGroup: IAsyncDisposable
{
    private readonly AwsSqsConnection            _connection;
    private HashSet<MinimalSqsClient.SqsMessage> _messages;
    private readonly PeriodicTimer               _timer;
    private readonly Task                        _updateVisibilityTask;

    public MessageGroup(AwsSqsConnection connection)
    {
        _connection           = connection;
        _timer                = new PeriodicTimer(TimeSpan.FromSeconds(_connection.Configuration.RenewVisibilityWaitTime));
        _updateVisibilityTask = UpdateVisibility();
    }
    public IEnumerable<PrefetchedSqsMessage> Initialize(List<MinimalSqsClient.SqsMessage>? messages)
    {
        if (messages is null) yield break;
        _messages = messages.ToHashSet();
        for (int i = 0; i < messages.Count; i++)
        {
            yield return new PrefetchedSqsMessage(this, messages[i]);
        }
    }
    private async Task UpdateVisibility()
    {
        var updatevisibilityTask = Task.CompletedTask;
        while (await _timer.WaitForNextTickAsync())
        {
            await updatevisibilityTask;
            var requestEntries = GetRequestsEntries();
            if (requestEntries.Length is 0) continue;
            updatevisibilityTask = _connection._sqsClient.ChangeMessageVisibilityBatchAsync(requestEntries, _connection.Configuration.VisibilityTimeout);
        }
    }

    private string[] GetRequestsEntries()
    {
        lock (this)
        {
            return _messages.Select(m => m.ReceiptHandle).ToArray(); 
        }
    }

    private ValueTask Remove(MinimalSqsClient.SqsMessage message)
    {
        bool dispose = false;
        lock (this)
        {
            _messages.Remove(message);
            dispose = _messages.Count is 0;
        }
        if (dispose)
        {
            return DisposeAsync();
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _timer.Dispose();
        return new ValueTask(_updateVisibilityTask);
    }

    internal class PrefetchedSqsMessage : SqsMessage
    {
        private readonly MessageGroup _messageGroup;

        public PrefetchedSqsMessage(MessageGroup messageGroup, MinimalSqsClient.SqsMessage internalMessage)
            :base(internalMessage)
        {
            _messageGroup = messageGroup;
        }
        public override BinaryData GetBody()
        {
            return new BinaryData(InternalMessage.Body);
        }
        public override ValueTask DisposeAsync()
        {
            return _messageGroup.Remove(InternalMessage);
        }
    }
}