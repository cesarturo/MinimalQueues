using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;
using MsOptions = Microsoft.Extensions.Options;

namespace MinimalQueues;

public static class BuilderOfQueueProcessorOptionsExtensions
{
    public static IOptionsBuilder<HandlerOptions> UseDeserializedMessageHandler(this IOptionsBuilder<QueueProcessorOptions> optionsBuilder)
    {
        optionsBuilder.ConfigureServices(services => services.TryAddSingleton<IDeserializer, Deserializer>());
        
        optionsBuilder.Configure((QueueProcessorOptions queueprocessorOptions, MsOptions.IOptionsMonitor<HandlerOptions> handlerOptions, IServiceProviderIsService isService) =>
        {//This code runs when the HostedService is instantiated
            var options = handlerOptions.Get(optionsBuilder.Name);
            var serializedMessageHandler = new DeserializeMessageHandler(options, isService);
            queueprocessorOptions.MessageHandlerDelegates.Add(serializedMessageHandler.Handle);
        });
        //DeserializedHandler Options will have the same name as QueueProcessor Options.
        //It should not be a problem since there is only one DeserializeMessageHandler per QueueProcessor:
        return optionsBuilder.AddOptions<HandlerOptions>(optionsBuilder.Name);
    }
    public static IOptionsBuilder<EndOptions> UseDeserializedMessageHandler(this IOptionsBuilder<QueueProcessorOptions> optionsBuilder
        , Delegate defaultHandlerDelegate)
    {
        var serializedHandlerOptions = optionsBuilder.UseDeserializedMessageHandler();
        return serializedHandlerOptions.Map(defaultHandlerDelegate);
    }
}