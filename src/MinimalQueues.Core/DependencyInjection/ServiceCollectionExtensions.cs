using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MsOptions = Microsoft.Extensions.Options;

namespace MinimalQueues.Core;

using Core.Options;
public static class ServiceCollectionExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddQueueProcessorHostedService(this IServiceCollection services)
    {
        return services.AddQueueProcessorHostedService(QueueProcessorNameGenerator.GetNewName());
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddQueueProcessorHostedService(this IServiceCollection services, string queueProcessorName)
    {
        services.AddSingleton<IHostedService>(serviceProvider =>
        {
            var options      = serviceProvider.GetRequiredService<MsOptions.IOptionsMonitor<QueueProcessorOptions>>()
                                              .Get(queueProcessorName);
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            var appLifeTime  = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            return new QueueProcessorHostedService(options, scopeFactory, appLifeTime);
        });
        return services.AddOptions<QueueProcessorOptions>(queueProcessorName);
    }
}