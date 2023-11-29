namespace MinimalQueues;

public sealed class HandlerOptions
{
    internal List<EndpointOptions> EndpointsOptions { get; set; } = new();
    internal EndpointOptions? UnhandledMessageEndpointOptions { get; set; }
    public Type? DeserializerType { get; set; }
    public IDeserializer? DeserializerInstance { get; set; }
}