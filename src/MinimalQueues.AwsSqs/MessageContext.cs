using Amazon.SQS.Model;

namespace MinimalQueues.AwsSqs;

internal interface IMessageContext : IAsyncDisposable
{
    public Message? Message { get; }
}

internal sealed class MessageContext : IMessageContext
{
    private readonly AwsSqsConnection _connection;
    private readonly PeriodicTimer _timer;
    private readonly Task _updateVisibilityTask;
    private volatile Message? _message;
    public Message? Message => _message;

    public MessageContext(AwsSqsConnection connection)
    {
        _connection = connection;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(_connection.RenewVisibilityWaitTime));
        _updateVisibilityTask = UpdateVisibility();
    }

    public void Initialize(Message message)
    {
        if (_message is null) _message = message;
        else throw new InvalidOperationException("Instance is already initialized.");
    }
    private async Task UpdateVisibility()
    {
        var updatevisibilityTask = Task.CompletedTask;
        while (await _timer.WaitForNextTickAsync())
        {
            await updatevisibilityTask;
            if (_message is null) continue;
            updatevisibilityTask = _connection.UpdateVisibility(_message);
        }
    }
    public ValueTask DisposeAsync()
    {
        _timer.Dispose();
        if (_updateVisibilityTask.IsCompletedSuccessfully)
            return ValueTask.CompletedTask;
        return new ValueTask(_updateVisibilityTask);
    }
}

//public static class CancellationTokenTask
//{
//    public static WaitCancellationToken CreateWaitCancellationToken(this CancellationToken cancellationToken)
//    {
//        return new WaitCancellationToken(cancellationToken);
//    }

//    public class WaitCancellationToken: IDisposable
//    {
//        private readonly TaskCompletionSource _taskCompletionSource;
//        private readonly CancellationTokenRegistration _registration;
//        public Task Task { get; }

//        public WaitCancellationToken(CancellationToken cancellationToken)
//        {
//            _taskCompletionSource = new TaskCompletionSource();
//            _registration = cancellationToken.Register(() => _taskCompletionSource.TrySetResult()
//                , useSynchronizationContext: false);
//            Task = _taskCompletionSource.Task;
//        }
//        public void Dispose()
//        {
//            _registration.Dispose();
//            _taskCompletionSource.TrySetResult();
//        }
//    }
//}
