using System.Text.Json;
using MinimalQueues.Core.Options;

namespace MinimalQueues;

public static class FluentExtensions
{
    public static IOptionsBuilder<HandlerOptions> ConfigureJson(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , JsonSerializerOptions jsonSerializerOptions)
    {
        return optionsBuilder.Configure(handlerOptions => handlerOptions.DeserializerInstance = new Deserializer(jsonSerializerOptions));
    }
    public static IOptionsBuilder<HandlerOptions> DeserializeWith<TDeserializer>(this IOptionsBuilder<HandlerOptions> optionsBuilder) where TDeserializer : IDeserializer
    {
        return optionsBuilder.Configure(options => options.DeserializerType = typeof(TDeserializer));
    }
    public static IOptionsBuilder<HandlerOptions> DeserializeWith(this IOptionsBuilder<HandlerOptions> optionsBuilder, IDeserializer deserializer)
    {
        return optionsBuilder.Configure(options => options.DeserializerInstance = deserializer);
    }
    public static IOptionsBuilder<EndpointOptions> Use(this IOptionsBuilder<HandlerOptions> optionsBuilder, Delegate handlerDelegate
        , string name)
    {
        optionsBuilder.Configure(delegate (HandlerOptions handlerOptions
            , Microsoft.Extensions.Options.IOptionsMonitor<EndpointOptions> endpointOptions)
        {
            handlerOptions.UnhandledMessageEndpointOptions = endpointOptions.Get(name);
        });
        return optionsBuilder.AddOptions<EndpointOptions>(name)
            .Configure(endpointOptions =>
            {
                endpointOptions.HandlerDelegate = handlerDelegate;
            });
    }
    public static IOptionsBuilder<EndpointOptions> Use(this IOptionsBuilder<HandlerOptions> optionsBuilder, Delegate handlerDelegate)
    {
        var name = EndpointOptionsNameGenerator.GetNewName();
        return optionsBuilder.Use(handlerDelegate, name);
    }
    public static IOptionsBuilder<EndpointOptions> When(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate match, string name)
    {
        optionsBuilder.Configure(delegate (HandlerOptions handlerOptions
            , Microsoft.Extensions.Options.IOptionsMonitor<EndpointOptions> endpointOptions)
        {
            handlerOptions.EndpointsOptions.Add(endpointOptions.Get(name));
        });
        return optionsBuilder.AddOptions<EndpointOptions>(name)
            .Configure(endpointOptions =>
            {
                endpointOptions.Match = MatchDelegateMetaBuilder.Build(match);
            });
    }
    public static IOptionsBuilder<EndpointOptions> When(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate match)
    {
        var optionsName = EndpointOptionsNameGenerator.GetNewName();
        return optionsBuilder.When(match, optionsName);
    }
    public static IOptionsBuilder<EndpointOptions> Use(this IOptionsBuilder<EndpointOptions> optionsBuilder, Delegate handlerDelegate)
    {
        return optionsBuilder.Configure(options => options.HandlerDelegate = handlerDelegate);
    }
    public static IOptionsBuilder<EndpointOptions> DeserializeWith<TDeserializer>(this IOptionsBuilder<EndpointOptions> optionsBuilder) where TDeserializer : IDeserializer
    {
        return optionsBuilder.Configure(options => options.DeserializerType = typeof(TDeserializer));
    }
    public static IOptionsBuilder<EndpointOptions> DeserializeWith(this IOptionsBuilder<EndpointOptions> optionsBuilder, IDeserializer deserializer)
    {
        return optionsBuilder.Configure(options => options.DeserializerInstance = deserializer);
    }
    public static IOptionsBuilder<EndpointOptions> ConfigureJson(this IOptionsBuilder<EndpointOptions> builder
        , JsonSerializerOptions jsonSerializerOptions)
    {
        return builder.Configure(options => options.DeserializerInstance = new Deserializer(jsonSerializerOptions));
    }
}