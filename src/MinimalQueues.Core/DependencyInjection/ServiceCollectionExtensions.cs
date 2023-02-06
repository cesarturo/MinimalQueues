using Microsoft.Extensions.DependencyInjection;
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
        services.AddHostedService(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<MsOptions.IOptionsMonitor<QueueProcessorOptions>>()
                .Get(queueProcessorName);
            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            return new QueueProcessorHostedService(options, serviceScopeFactory);
        });
        return services.AddOptions<QueueProcessorOptions>(queueProcessorName);
    }
}