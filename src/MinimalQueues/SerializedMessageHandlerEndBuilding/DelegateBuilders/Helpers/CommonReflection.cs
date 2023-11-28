using System.Reflection;
using MinimalQueues.Core;

namespace MinimalQueues;

internal sealed class CommonReflection
{
    public readonly Type       serviceProviderType;
    public readonly MethodInfo getServiceMethod;
    public readonly Type       MessageType;
    public readonly Type       BinaryDataType;
    public readonly MethodInfo DeserializeMethodGenericDefinition;
    public readonly MethodInfo GetPropertyMethodDefinition;

    public CommonReflection()
    {
        serviceProviderType = typeof(IServiceProvider);
        getServiceMethod    = serviceProviderType.GetMethod(nameof(IServiceProvider.GetService))!;
        MessageType         = typeof(IMessage);
        BinaryDataType      = typeof(BinaryData);
        DeserializeMethodGenericDefinition = typeof(IDeserializer)
            .GetMethod(nameof(IDeserializer.Deserialize)
                     , 1
                     , new[] { BinaryDataType })!;
        GetPropertyMethodDefinition = typeof(IMessageProperties)
            .GetMethod(nameof(IMessageProperties.GetProperty)
                     , 1
                     , new[] { typeof(string) })!;
    }
}