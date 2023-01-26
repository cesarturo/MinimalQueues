using Microsoft.Extensions.Hosting;
using MinimalQueues.Options;

namespace MinimalQueues;

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