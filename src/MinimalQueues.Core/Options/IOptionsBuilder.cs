using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues.Core.Options;

public interface IOptionsBuilder<TOptions> where TOptions : class
{
    string? Name { get; }
    IOptionsBuilder<TOptions> Configure(Action<TOptions> configureAction);

    IOptionsBuilder<TOptions> Configure<TDep>(Action<TOptions, TDep> configureOptions)
        where TDep : class;

    IOptionsBuilder<TOptions> Configure<TDep1, TDep2>(Action<TOptions, TDep1, TDep2> configureOptions)
        where TDep1 : class
        where TDep2 : class;

    IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3>(Action<TOptions, TDep1, TDep2, TDep3> configureOptions)
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class;

    IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3, TDep4>(Action<TOptions, TDep1, TDep2, TDep3, TDep4> configureOptions)
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class;

    IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3, TDep4, TDep5>(Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5> configureOptions)
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TDep5 : class;

    IOptionsBuilder<TOptions> ConfigureServices(Action<IServiceCollection> configureAction);
    IOptionsBuilder<TOtherOptions> AddOptions<TOtherOptions>(string? name = null) where TOtherOptions : class;
}