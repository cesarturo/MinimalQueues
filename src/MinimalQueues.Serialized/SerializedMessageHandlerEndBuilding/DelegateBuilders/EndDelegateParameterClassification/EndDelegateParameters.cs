using System.Reflection;

namespace MinimalQueues.Deserialization;

internal class EndDelegateParameters
{
    internal (ParameterInfo parameterInfo, ParameterKind type)[] ParametersClassified { get; }
    internal ParameterInfo? BodyParameter { get; }
    internal EndDelegateParameters((ParameterInfo parameterInfo, ParameterKind type)[] parametersClassified, ParameterInfo? bodyParameter)
    {
        ParametersClassified = parametersClassified;
        BodyParameter = bodyParameter;
    }
}