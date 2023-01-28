namespace MinimalQueues
{
    internal static class HandlerDelegateBuilder
    {
        public static Func<IMessage, IServiceProvider, Task> Build(List<MessageHandlerDelegate> messageHandlerDelegates, CancellationToken token)
        {
            Func<IMessage, IServiceProvider, Task>? handlerDelegate = null;
            int i = messageHandlerDelegates.Count - 1;
            do
            {
                var messageHandlerDelegate = messageHandlerDelegates[i];
                var next = handlerDelegate;
                handlerDelegate =  next is null 
                    ? (msj, serviceProvider) => messageHandlerDelegate(msj, null, serviceProvider, token)
                    : (msj, serviceProvider) => messageHandlerDelegate(msj, msjParam => next(msjParam, serviceProvider), serviceProvider, token);
            } while (i-- > 0);
            return handlerDelegate;
        }
    }
}
