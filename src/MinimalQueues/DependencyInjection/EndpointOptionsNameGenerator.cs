namespace MinimalQueues;

internal class EndpointOptionsNameGenerator
{
    internal static int i;
    public static string GetNewName() => $"end{i++}";
}