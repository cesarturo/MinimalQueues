using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MsOptions= Microsoft.Extensions.Options;

namespace MinimalQueues.Options
{
    using static Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions;
    public sealed class HostOptionsBuilder<TOptions> : IOptionsBuilder<TOptions> where TOptions : class
    {
        public IHostBuilder HostBuilder { get; }
        public string? Name { get; }

        public HostOptionsBuilder(IHostBuilder hostBuilder, string? name = null)
        {
            HostBuilder = hostBuilder;
            Name = name ?? MsOptions.Options.DefaultName;
        }

        public IOptionsBuilder<TOptions> Configure(Action<TOptions> configureAction)
        {
            HostBuilder.ConfigureServices((ctx, services) => services.Configure<TOptions>(Name, configureAction));
            return this;
        }
        public IOptionsBuilder<TOptions> Configure<TDep>(Action<TOptions, TDep> configureOptions)
            where TDep : class
        {
            HostBuilder.ConfigureServices((ctx, services) => services.AddOptions<TOptions>(Name).Configure(configureOptions));
            return this;
        }
        public IOptionsBuilder<TOptions> Configure<TDep1, TDep2>(Action<TOptions, TDep1, TDep2> configureOptions)
            where TDep1 : class
            where TDep2 : class
        {
            HostBuilder.ConfigureServices((ctx, services) => services.AddOptions<TOptions>(Name).Configure(configureOptions));
            return this;
        }
        public IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3>(Action<TOptions, TDep1, TDep2, TDep3> configureOptions)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
        {
            HostBuilder.ConfigureServices((ctx, services) => services.AddOptions<TOptions>(Name).Configure(configureOptions));
            return this;
        }
        public IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3, TDep4>(Action<TOptions, TDep1, TDep2, TDep3, TDep4> configureOptions)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
            where TDep4 : class
        {
            HostBuilder.ConfigureServices((ctx, services) => services.AddOptions<TOptions>(Name).Configure(configureOptions));
            return this;
        }
        public IOptionsBuilder<TOptions> Configure<TDep1, TDep2, TDep3, TDep4, TDep5>(Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5> configureOptions)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
            where TDep4 : class
            where TDep5 : class
        {
            HostBuilder.ConfigureServices((ctx, services) => services.AddOptions<TOptions>(Name).Configure(configureOptions));
            return this;
        }

        public IOptionsBuilder<TOptions> ConfigureServices(Action<IServiceCollection> configureAction)
        {
            HostBuilder.ConfigureServices((ctx, services) => configureAction(services));
            return this;
        }
        public IOptionsBuilder<TOtherOptions> AddOptions<TOtherOptions>(string? name = null) where TOtherOptions : class
        {
            HostBuilder.ConfigureServices((ctx, services) => OptionsServiceCollectionExtensions.AddOptions(services));//just to make sure Microsoft Options infrastructure is added
            return new HostOptionsBuilder<TOtherOptions>(HostBuilder, name);
        }
    }
}
