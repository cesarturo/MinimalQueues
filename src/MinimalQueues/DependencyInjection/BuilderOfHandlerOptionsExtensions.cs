using MinimalQueues.Core;
using MinimalQueues.Core.Options;
using MsOptions = Microsoft.Extensions.Options;

namespace MinimalQueues;

public static class BuilderOfHandlerOptionsExtensions
{
    public static IOptionsBuilder<EndpointOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate handlerDelegate
        , string name
        , Type? deserializerType = null
        , IDeserializer? deserializerInstance = null)
    {
        if (deserializerType is not null && !typeof(IDeserializer).IsAssignableFrom(deserializerType))
            throw new ArgumentException($"deserializerType must implement {nameof(IDeserializer)}", nameof(deserializerType));

        optionsBuilder.Configure(delegate (HandlerOptions handlerOptions, MsOptions.IOptionsMonitor<EndpointOptions> endpointOptions)
        {
            //Runs when HostedService is instantiated, since the HostedService depends on Serialized Handler which depends on this options
            handlerOptions.UnhandledMessageEndpointOptions = endpointOptions.Get(name);
        });
        return optionsBuilder.AddOptions<EndpointOptions>(name)
            .Configure(endpointOptions =>
            {
                endpointOptions.HandlerDelegate = handlerDelegate;
                endpointOptions.DeserializerType = deserializerType;
                endpointOptions.DeserializerInstance = deserializerInstance;
            });
    }
    public static IOptionsBuilder<EndpointOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate handlerDelegate
        , Type? deserializerType = null
        , IDeserializer? deserializerInstance = null)
    {
        var optionsName = EndpointOptionsNameGenerator.GetNewName();
        return optionsBuilder.Map(handlerDelegate, optionsName, deserializerType, deserializerInstance);
    }
    public static IOptionsBuilder<EndpointOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Func<IMessageProperties, bool> match
        , Delegate handlerDelegate
        , string name
        , Type? deserializerType = null
        , IDeserializer? deserializerInstance = null)
    {
        if (deserializerType is not null && !typeof(IDeserializer).IsAssignableFrom(deserializerType))
            throw new ArgumentException($"deserializerType must implement {nameof(IDeserializer)}", nameof(deserializerType));

        optionsBuilder.Configure(delegate (HandlerOptions handlerOptions
            , MsOptions.IOptionsMonitor<EndpointOptions> endpointOptions)
        {
            //Runs when HostedService is instantiated, since the HostedService depends on Serialized Handler which depends on this options
            handlerOptions.EndpointsOptions.Add(endpointOptions.Get(name));
        });

        return optionsBuilder.AddOptions<EndpointOptions>(name)
            .Configure(endpointOptions =>
            {
                endpointOptions.Match = match;
                endpointOptions.HandlerDelegate = handlerDelegate;
                endpointOptions.DeserializerType = deserializerType;
                endpointOptions.DeserializerInstance = deserializerInstance;
            });
    }
    
    public static IOptionsBuilder<EndpointOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Func<IMessageProperties, bool> match
        , Delegate handlerDelegate)
    {
        return optionsBuilder.Map(match, handlerDelegate, EndpointOptionsNameGenerator.GetNewName());
    }

    public static IOptionsBuilder<EndpointOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate match
        , Delegate handlerDelegate)
    {
        return optionsBuilder.Map(MatchDelegateMetaBuilder.Build(match), handlerDelegate, EndpointOptionsNameGenerator.GetNewName());
    }

    public static IOptionsBuilder<EndpointOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate match
        , Delegate handlerDelegate
        , string name
        , Type? deserializerType = null
        , IDeserializer? deserializerInstance = null)
    {
        return optionsBuilder.Map(MatchDelegateMetaBuilder.Build(match), handlerDelegate, name, deserializerType, deserializerInstance);
    }
}