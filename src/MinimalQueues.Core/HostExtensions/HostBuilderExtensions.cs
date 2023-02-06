using Microsoft.Extensions.Hosting;
using MinimalQueues.Core.Options;

namespace MinimalQueues.Core;

public static class HostBuilderExtensions
{
    public static IOptionsBuilder<QueueProcessorOptions> AddQueueProcessorHostedService(this IHostBuilder builder)
    {
        return builder.AddQueueProcessorHostedService(QueueProcessorNameGenerator.GetNewName());
    }
    public static IOptionsBuilder<QueueProcessorOptions> AddQueueProcessorHostedService(this IHostBuilder builder, string queueProcessorName)
    {
        builder.ConfigureServices((ctx, services) => services.AddQueueProcessorHostedService(queueProcessorName));
        return builder.AddOptions<QueueProcessorOptions>(queueProcessorName);
    }
}