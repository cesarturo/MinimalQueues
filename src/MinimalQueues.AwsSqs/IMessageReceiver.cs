namespace MinimalQueues.AwsSqs;

internal interface IMessageReceiver: IDisposable
{
    Task<SqsMessage?> ReceiveMessage(CancellationToken cancellation);
}