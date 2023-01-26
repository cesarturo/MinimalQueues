namespace MinimalQueues
{
    internal static class ParameterlessHandlerBuilder
    {
        public static Func<IMessage, Task> Build(List<MessageHandlerDelegate> messageHandlerDelegates, IMessage message, IServiceProvider serviceProvider, CancellationToken token)
        {
            Func<IMessage, Task>? parameterlessHandlerDelegate = null;
            int i = messageHandlerDelegates.Count - 1;
            do
            {
                var messageHandlerDelegate = messageHandlerDelegates[i];
                var next = parameterlessHandlerDelegate; 
                //Not using async await can have effects in the Stack Trace of Exceptions that could occur
                parameterlessHandlerDelegate = /*async*/ (msj) => /*await*/ messageHandlerDelegate(msj, next, serviceProvider, token);
            } while (i-- > 0);
            return parameterlessHandlerDelegate;
        }
    }
}
