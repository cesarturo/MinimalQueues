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
    public static IOptionsBuilder<EndOptions> Use(this IOptionsBuilder<HandlerOptions> optionsBuilder, Delegate handlerDelegate
        , string name)
    {
        optionsBuilder.Configure(delegate (HandlerOptions handlerOptions
            , Microsoft.Extensions.Options.IOptionsMonitor<EndOptions> endOptions)
        {
            handlerOptions.UnhandledMessageEndOptions = endOptions.Get(name);
        });
        return optionsBuilder.AddOptions<EndOptions>(name)
            .Configure(endOptions =>
            {
                endOptions.HandlerDelegate = handlerDelegate;
            });
    }
    public static IOptionsBuilder<EndOptions> Use(this IOptionsBuilder<HandlerOptions> optionsBuilder, Delegate handlerDelegate)
    {
        var name = EndNameGenerator.GetNewName();
        return optionsBuilder.Use(handlerDelegate, name);
    }
    public static IOptionsBuilder<EndOptions> When(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate match, string name)
    {
        optionsBuilder.Configure(delegate (HandlerOptions handlerOptions
            , Microsoft.Extensions.Options.IOptionsMonitor<EndOptions> endOptions)
        {
            handlerOptions.Ends.Add(endOptions.Get(name));
        });
        return optionsBuilder.AddOptions<EndOptions>(name)
            .Configure(endOptions =>
            {
                endOptions.Match = MatchMetaBuilder.Build(match);
            });
    }
    public static IOptionsBuilder<EndOptions> When(this IOptionsBuilder<HandlerOptions> optionsBuilder
        , Delegate match)
    {
        var name = EndNameGenerator.GetNewName();
        return optionsBuilder.When(match, name);
    }
    public static IOptionsBuilder<EndOptions> Use(this IOptionsBuilder<EndOptions> optionsBuilder, Delegate handlerDelegate)
    {
        return optionsBuilder.Configure(options => options.HandlerDelegate = handlerDelegate);
    }
    public static IOptionsBuilder<EndOptions> DeserializeWith<TDeserializer>(this IOptionsBuilder<EndOptions> optionsBuilder) where TDeserializer : IDeserializer
    {
        return optionsBuilder.Configure(options => options.DeserializerType = typeof(TDeserializer));
    }
    public static IOptionsBuilder<EndOptions> ConfigureJson(this IOptionsBuilder<EndOptions> builder
        , JsonSerializerOptions jsonSerializerOptions)
    {
        return builder.Configure(options => options.DeserializerInstance = new Deserializer(jsonSerializerOptions));
    }
}