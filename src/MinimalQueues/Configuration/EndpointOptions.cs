using MinimalQueues.Core;

namespace MinimalQueues;

public sealed class EndpointOptions
{
#pragma warning disable CS8618
    public Func<IMessageProperties, bool> Match { get; set; }
    public Type? DeserializerType { get; set; }
    public IDeserializer? DeserializerInstance { get; set; }
    public Delegate HandlerDelegate { get; set; }
#pragma warning restore CS8618
}