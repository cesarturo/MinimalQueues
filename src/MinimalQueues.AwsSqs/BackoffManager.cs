namespace MinimalQueues.AwsSqs;

internal class BackoffManager
{
    private readonly AwsSqsConnection _connection;
    private int                       _backoffCount;
    private volatile SemaphoreSlim?   _semaphore;
    private volatile PeriodicTimer?   _timer;
    private volatile Status           _status;
    private int                       _semaphoreReleaseCount;
    private Task                      _timerWaitTask;
    
    public BackoffManager(AwsSqsConnection connection)
    {
        _connection = connection;
        _status = Status.Releasing;
        _semaphore = new SemaphoreSlim(1);
        _semaphoreReleaseCount = 1;
    }

    private async Task TimerWaitTask()
    {
        var timer = _timer;
        if (timer is null) return;
        await timer.WaitForNextTickAsync(_connection.Cancellation);
        lock (this)
        {
            if (_status is not Status.Waiting) return;
            DisposeTimerNoThreadSafe();
            _semaphore!.Release();
            _status = Status.Releasing;
            _semaphoreReleaseCount++;
        }
    }

    public ValueTask Wait()
    {
        if (_status is Status.Disabled) return ValueTask.CompletedTask;
        try
        {
            return new ValueTask(_semaphore!.WaitAsync(_connection.Cancellation));
        }
        catch (NullReferenceException exception)
        {
            return ValueTask.CompletedTask;
        }
    }

    public void Start()
    {
        if (_status is Status.Waiting) return;
        lock (this)
        {
            if (_status is Status.Waiting) return;
            var waitTime = _connection.Configuration.BackOffFunction(++_backoffCount);
            DisposeTimerNoThreadSafe();
            _semaphore ??= new SemaphoreSlim(0);
            _semaphoreReleaseCount = 0;
            _timer = new PeriodicTimer(waitTime);
            _status = Status.Waiting;
            _timerWaitTask = TimerWaitTask();
        }
    }

    public void Release()
    {
        if (_status is Status.Disabled) return;
        lock (this)
        {
            if (_status is Status.Disabled) return;
            _backoffCount = 0;
            var releaseQuantity = _semaphoreReleaseCount is 0 ? 1 : 2;
            _semaphore!.Release(releaseQuantity);
            _semaphoreReleaseCount += releaseQuantity;
            DisposeTimerNoThreadSafe();
            if (_semaphoreReleaseCount >= _connection.Configuration.MaxConcurrentCalls)
            {
                _semaphore.Release(int.MaxValue - _semaphore.CurrentCount);
                _semaphore.Dispose();
                _semaphore = null;
                _status = Status.Disabled;
            }
        }
    }

    private void DisposeTimerNoThreadSafe()
    {
        if (_timer is not null)
        {
            _timer.Dispose();
            _timer = null;
        }
    }

    internal enum Status
    {
        Disabled, Waiting, Releasing
    }
}