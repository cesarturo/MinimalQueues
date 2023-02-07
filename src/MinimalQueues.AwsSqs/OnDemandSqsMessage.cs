﻿using Amazon.SQS.Model;

namespace MinimalQueues.AwsSqs;

internal sealed class OnDemandSqsMessage : SqsMessage
{
    private readonly AwsSqsConnection _connection;
    private readonly PeriodicTimer    _timer;
    private readonly Task             _updateVisibilityTask;

    public OnDemandSqsMessage(Message internalMessage, AwsSqsConnection connection, PeriodicTimer renewTimer)
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
            updatevisibilityTask = _connection.UpdateVisibility(InternalMessage);
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