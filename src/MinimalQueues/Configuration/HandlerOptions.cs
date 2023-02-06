namespace MinimalQueues;

public sealed class HandlerOptions
{
    internal List<EndOptions> Ends { get; set; } = new();
    internal EndOptions? UnhandledMessageEndOptions { get; set; }
    public Type? DeserializerType { get; set; }
    public IDeserializer? DeserializerInstance { get; set; }
}