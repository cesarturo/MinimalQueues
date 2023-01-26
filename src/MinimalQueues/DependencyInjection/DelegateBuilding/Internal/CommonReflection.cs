using System.Reflection;

namespace MinimalQueues;
internal sealed class CommonReflection
{
    public readonly Type       ServiceProviderType;
    public readonly MethodInfo GetServiceMethod;
    public readonly Type       MessageType;
    public readonly MethodInfo GetPropertyMethodDefinition;
    public CommonReflection()
    {
        ServiceProviderType         = typeof(IServiceProvider);
        GetServiceMethod            = ServiceProviderType.GetMethod(nameof(IServiceProvider.GetService))!;
        MessageType                 = typeof(IMessage);
        GetPropertyMethodDefinition = typeof(IMessageProperties).GetMethod(nameof(IMessageProperties.GetProperty)
                                    , 1
                                    , new[] { typeof(string) })!;
    }

    
}