namespace MinimalQueues.Deserialization;

internal class EndNameGenerator
{
    internal static int i;
    public static string GetNewName() => $"end{i++}";
}