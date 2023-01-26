using MinimalQueues.Options;
using MsOptions = Microsoft.Extensions.Options;

namespace MinimalQueues.Deserialization;

public static class BuilderOfHandlerOptionsExtensions
{
    public static IOptionsBuilder<EndOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate handlerDelegate
        , string name
        , Type? deserializerType = null
        , IDeserializer? deserializerInstance = null)
    {
        if (deserializerType is not null && !typeof(IDeserializer).IsAssignableFrom(deserializerType))
            throw new ArgumentException($"deserializerType must implement {nameof(IDeserializer)}", nameof(deserializerType));

        optionsBuilder.Configure(delegate (HandlerOptions handlerOptions, MsOptions.IOptionsMonitor<EndOptions> endOptions)
        {
            //Runs when HostedService is instantiated, since the HostedService depends on Serialized Handler which depends on this options
            handlerOptions.UnhandledMessageEndOptions = endOptions.Get(name);
        });
        return optionsBuilder.AddOptions<EndOptions>(name)
            .Configure(endOptions =>
            {
                endOptions.HandlerDelegate = handlerDelegate;
                endOptions.DeserializerType = deserializerType;
                endOptions.DeserializerInstance = deserializerInstance;
            });
    }
    public static IOptionsBuilder<EndOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate handlerDelegate
        , Type? deserializerType = null
        , IDeserializer? deserializerInstance = null)
    {
        var endName = EndNameGenerator.GetNewName();
        return optionsBuilder.Map(handlerDelegate, endName, deserializerType, deserializerInstance);
    }
    public static IOptionsBuilder<EndOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Func<IMessageProperties, bool> match
        , Delegate handlerDelegate
        , string name
        , Type? deserializerType = null
        , IDeserializer? deserializerInstance = null)
    {
        if (deserializerType is not null && !typeof(IDeserializer).IsAssignableFrom(deserializerType))
            throw new ArgumentException($"deserializerType must implement {nameof(IDeserializer)}", nameof(deserializerType));

        optionsBuilder.Configure(delegate (HandlerOptions handlerOptions
            , MsOptions.IOptionsMonitor<EndOptions> endOptions)
        {
            //Runs when HostedService is instantiated, since the HostedService depends on Serialized Handler which depends on this options
            handlerOptions.Ends.Add(endOptions.Get(name));
        });

        return optionsBuilder.AddOptions<EndOptions>(name)
            .Configure(endOptions =>
            {
                endOptions.Match = match;
                endOptions.HandlerDelegate = handlerDelegate;
                endOptions.DeserializerType = deserializerType;
                endOptions.DeserializerInstance = deserializerInstance;
            });
    }
    
    public static IOptionsBuilder<EndOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Func<IMessageProperties, bool> match
        , Delegate handlerDelegate)
    {
        return optionsBuilder.Map(match, handlerDelegate, EndNameGenerator.GetNewName());
    }

    public static IOptionsBuilder<EndOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate match
        , Delegate handlerDelegate)
    {
        return optionsBuilder.Map(MatchMetaBuilder.Build(match), handlerDelegate, EndNameGenerator.GetNewName());
    }

    public static IOptionsBuilder<EndOptions> Map(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate match
        , Delegate handlerDelegate
        , string name
        , Type? deserializerType = null
        , IDeserializer? deserializerInstance = null)
    {
        return optionsBuilder.Map(MatchMetaBuilder.Build(match), handlerDelegate, name, deserializerType, deserializerInstance);
    }
}