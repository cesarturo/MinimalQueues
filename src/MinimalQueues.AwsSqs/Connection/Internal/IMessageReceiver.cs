namespace MinimalQueues.AwsSqs.Connection.Internal;

internal interface IMessageReceiver: IDisposable
{
    Task<SqsMessage?> ReceiveMessage(CancellationToken cancellation);
}