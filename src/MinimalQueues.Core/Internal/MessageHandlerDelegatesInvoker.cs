namespace MinimalQueues.Core
{
    internal class MessageHandlerDelegatesInvoker
    {
        private readonly List<MessageHandlerDelegate> _messageHandlerDelegates;
        private readonly CancellationToken _token;
        private int _i;
        private IServiceProvider? _provider;
        internal MessageHandlerDelegatesInvoker(List<MessageHandlerDelegate> messageHandlerDelegates, CancellationToken token)
        {
            _messageHandlerDelegates = messageHandlerDelegates;
            _token = token;
        }
        public Task Handle(IMessage message, IServiceProvider provider)
        {
            _provider = provider;
            return HandleN(message, provider);
        }
        private Task HandleN(IMessage message, IServiceProvider provider)
        {
            var next = _i + 1 < _messageHandlerDelegates.Count ? Next : (Func<IMessage, Task>?)null;
            return _messageHandlerDelegates[_i++](message, next, provider, _token);
        }
        private Task Next(IMessage message)
        {
            return HandleN(message, _provider!);
        }
    }
}
