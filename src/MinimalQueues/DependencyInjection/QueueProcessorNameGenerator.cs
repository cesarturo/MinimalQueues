namespace MinimalQueues;

internal static class QueueProcessorNameGenerator
{
    private static int i;
    public static string GetNewName() => $"queueprocessor{i++}";
}