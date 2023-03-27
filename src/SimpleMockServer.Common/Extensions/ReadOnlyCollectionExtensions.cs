namespace SimpleMockServer.Common.Extensions;

public static class ReadOnlyCollectionExtensions
{
    public static int GetMaxIndex<T>(this IReadOnlyCollection<T> items)
    {
        return items.Count - 1;
    }
}