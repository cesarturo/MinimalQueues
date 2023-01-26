using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MinimalQueues.Options;

using static Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions;
public static class OptionsBuilderExtensions
{
    public static IOptionsBuilder<TOptions> AddOptions<TOptions>(this IServiceCollection services, string? name = null) where TOptions : class
    {
        return new ServiceCollectionOptionsBuilder<TOptions>(services, name);
    }
        
    public static IOptionsBuilder<TOptions> AddOptions<TOptions>(this IHostBuilder hostBuilder, string? name = null) where TOptions : class
    {
        hostBuilder.ConfigureServices((ctx, services)=> services.AddOptions());//just to make sure Microsoft Options infrastructure is added
        return new HostOptionsBuilder<TOptions>(hostBuilder, name);
    }
}