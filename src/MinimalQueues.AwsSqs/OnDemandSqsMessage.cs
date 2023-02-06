using Amazon.SQS.Model;

namespace MinimalQueues.AwsSqs;

internal sealed class OnDemandSqsMessage : SqsMessage
{
    private readonly AwsSqsConnection _connection;
    private readonly PeriodicTimer    _timer;
    private readonly Task             _updateVisibilityTask;

    public OnDemandSqsMessage(Message innerMessage, AwsSqsConnection connection, PeriodicTimer renewTimer)
        :base(innerMessage)
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
            if (InnerMessage is null) continue;
            updatevisibilityTask = _connection.UpdateVisibility(InnerMessage);
        }
    }
    public override BinaryData GetBody() => new BinaryData(InnerMessage.Body);


    public override ValueTask DisposeAsync()
    {
        _timer.Dispose();
        if (_updateVisibilityTask.IsCompletedSuccessfully)
            return ValueTask.CompletedTask;
        return new ValueTask(_updateVisibilityTask);
    }
}