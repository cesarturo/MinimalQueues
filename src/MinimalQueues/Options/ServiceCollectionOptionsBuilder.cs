using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MinimalQueues.Options;

public sealed class ServiceCollectionOptionsBuilder<TOptions> : IOptionsBuilder<TOptions> where TOptions : class
{
    public OptionsBuilder<TOptions> InnerOptionsBuilder { get; }
    public string? Name => InnerOptionsBuilder.Name;

    public ServiceCollectionOptionsBuilder(OptionsBuilder<TOptions> innerOptionsBuilder)
    {
        InnerOptionsBuilder = innerOptionsBuilder;
    }
    public ServiceCollectionOptionsBuilder(IServiceCollection services, string? name = null)
    {
        InnerOptionsBuilder = OptionsServiceCollectionExtensions.AddOptions<TOptions>(services, name);
    }

    public IOptionsBuilder<TOptions> Configure(Action<TOptions> configureAction)
    {
        InnerOptionsBuilder.Configure(configureAction);
        return this;
    }
    public IOptionsBuilder<TOptions> Configure<TDep>(Action<TOptions, TDep> configureOptions)
        where TDep : class
    {
        InnerOptionsBuilder.Configure(configureOptions);
        return this;
    }
    public IOptionsBuilder<TOptions> Configure<TDep1, TDep2>(Action<TOptions, TDep1, TDep2> configureOptions)
        where TDep1 : class
        where TDep2 : class
    {
        InnerOptionsBuilder.Configure(configureOptions);
        return this;
    }
    public IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3>(Action<TOptions, TDep1, TDep2, TDep3> configureOptions)
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
    {
        InnerOptionsBuilder.Configure(configureOptions);
        return this;
    }
    public IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3, TDep4>(Action<TOptions, TDep1, TDep2, TDep3, TDep4> configureOptions)
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
    {
        InnerOptionsBuilder.Configure(configureOptions);
        return this;
    }
    public IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3, TDep4, TDep5>(Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5> configureOptions)
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TDep5 : class
    {
        InnerOptionsBuilder.Configure(configureOptions);
        return this;
    }

    public IOptionsBuilder<TOptions> ConfigureServices(Action<IServiceCollection> configureAction)
    {
        configureAction(InnerOptionsBuilder.Services);
        return this;
    }
    public IOptionsBuilder<TOtherOptions> AddOptions<TOtherOptions>(string? name = null) where TOtherOptions : class
    {
        var otherInnerOptions = OptionsServiceCollectionExtensions.AddOptions<TOtherOptions>(InnerOptionsBuilder.Services, name);
        return new ServiceCollectionOptionsBuilder<TOtherOptions>(otherInnerOptions);
    }
}