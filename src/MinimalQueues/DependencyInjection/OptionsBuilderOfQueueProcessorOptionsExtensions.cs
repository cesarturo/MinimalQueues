using Microsoft.Extensions.DependencyInjection;
using MinimalQueues.Options;

namespace MinimalQueues;

public static class OptionsBuilderOfQueueProcessorOptionsExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> Use(this IOptionsBuilder<QueueProcessorOptions> optionsBuilder
        , MessageHandlerDelegate handlerDelegate)
    {
        return optionsBuilder.Configure(options =>
        {
            options.MessageHandlerDelegates.Add(handlerDelegate);
        });
    }
    public static IOptionsBuilder<QueueProcessorOptions> Use(this IOptionsBuilder<QueueProcessorOptions> optionsBuilder
        , Delegate handlerDelegate)
    {
        return optionsBuilder.Configure((QueueProcessorOptions options, IServiceProviderIsService isService) =>
        {
            var messageHandlerDelegate = HandlerDelegateMetaBuilder.Build(handlerDelegate, isService);
            options.MessageHandlerDelegates.Add(messageHandlerDelegate);
        });
    }
}