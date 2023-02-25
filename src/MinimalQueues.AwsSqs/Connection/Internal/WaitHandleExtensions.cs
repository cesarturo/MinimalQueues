namespace MinimalQueues.AwsSqs.Connection.Internal;

internal static class WaitHandleExtensions
{
    public static Task WaitAsync(this WaitHandle waitHandle, CancellationToken? cancellation=null)
    {
        var alreadySignaled = waitHandle.WaitOne(0);
        if (alreadySignaled) return Task.CompletedTask;

        var taskCompletionSource = new TaskCompletionSource();

        var waitHandleRegistration = ThreadPool.RegisterWaitForSingleObject(waitHandle
            , (state, timedOut) => ((TaskCompletionSource)state).TrySetResult()
            , taskCompletionSource
            , Timeout.InfiniteTimeSpan, executeOnlyOnce: true);

        var cancellationRegistration = cancellation?.Register(state => ((TaskCompletionSource)state).TrySetCanceled()
            , taskCompletionSource
            , useSynchronizationContext: false);

        return WaitAndUnregister(taskCompletionSource.Task, waitHandleRegistration, cancellationRegistration);
    }

    public static Task WaitAsync(this WaitHandle waitHandle, TimeSpan timeout)
    {
        return Task.WhenAny(waitHandle.WaitAsync(), Task.Delay(timeout));
    }
    private static async Task WaitAndUnregister(Task task, RegisteredWaitHandle registration, CancellationTokenRegistration? cancellationRegistration)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        finally
        {
            registration.Unregister(null);
            cancellationRegistration?.Dispose();
        }
    }
}