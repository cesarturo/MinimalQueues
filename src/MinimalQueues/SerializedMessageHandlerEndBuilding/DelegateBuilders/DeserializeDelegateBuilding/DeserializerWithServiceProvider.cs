using Microsoft.Extensions.DependencyInjection;

namespace MinimalQueues;

internal static class DeserializerWithServiceProvider 
{
    public static T? Deserialize<TDeserializer,T>(IServiceProvider serviceProvider, BinaryData data)
        where TDeserializer : IDeserializer
    {
        return serviceProvider.GetRequiredService<TDeserializer>().Deserialize<T>(data);
    }
}