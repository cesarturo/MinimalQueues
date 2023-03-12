namespace MinimalQueues.AwsSqs.Connection.Internal.OnDemand;

internal sealed class OnDemandSqsMessage : SqsMessage
{
    private readonly AwsSqsConnection _connection;
    private readonly PeriodicTimer    _timer;
    private readonly Task             _updateVisibilityTask;

    public OnDemandSqsMessage(MinimalSqsClient.SqsMessage internalMessage, AwsSqsConnection connection, PeriodicTimer renewTimer)
        :base(internalMessage)
    {
        _connection = connection;
        _timer = renewTimer;
        _updateVisibilityTask = UpdateVisibility();
    }
    private async Task UpdateVisibility()
    {
        var updatevisibilityTask = Task.CompletedTask;
        while (await _timer.WaitForNextTickAsync())
        {
            await updatevisibilityTask;
            if (InternalMessage is null) continue;
            updatevisibilityTask = _connection.UpdateVisibilityAsync(InternalMessage);
        }
    }
    public override BinaryData GetBody() => new BinaryData(InternalMessage.Body);


    public override ValueTask DisposeAsync()
    {
        _timer.Dispose();
        if (_updateVisibilityTask.IsCompletedSuccessfully)
            return ValueTask.CompletedTask;
        return new ValueTask(_updateVisibilityTask);
    }
}