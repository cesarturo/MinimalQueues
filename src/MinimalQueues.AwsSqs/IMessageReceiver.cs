namespace MinimalQueues.AwsSqs;

internal interface IMessageReceiver
{
    Task<IMessageContext?> ReceiveMessage(CancellationToken cancellation);
}