namespace MinimalQueues;

[AttributeUsage(AttributeTargets.Parameter)]
public class PropAttribute : Attribute
{
    public readonly string PropertyName;

    public PropAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}