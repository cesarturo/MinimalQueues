using System.Reflection;

namespace MinimalQueues;

internal class EndDelegateMetadata
{
    internal EndDelegateParameterInfo[] ParametersClassified { get; }
    internal ParameterInfo? BodyParameter { get; }
    internal EndDelegateMetadata(EndDelegateParameterInfo[] parametersClassified, ParameterInfo? bodyParameter)
    {
        ParametersClassified = parametersClassified;
        BodyParameter = bodyParameter;
    }
}
internal struct EndDelegateParameterInfo
{
    public ParameterInfo ParameterInfo { get; }
    public ParameterKind Type { get; }

    public EndDelegateParameterInfo(ParameterInfo parameterInfo, ParameterKind type)
    {
        ParameterInfo = parameterInfo;
        Type = type;
    }
}

internal enum ParameterKind : byte
{
    service,
    prop,
    cancellation,
    body
}