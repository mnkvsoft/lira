namespace SimpleMockServer.Common.Extensions;

public static class SetExtensions
{
    public static void AddOrThrowIfContains<T>(this ISet<T> set, T item)
    {
        if (set.Contains(item))
            throw new InvalidOperationException($"Item {item} already contains in set");
        set.Add(item);
    }

    public static void AddRangeOrThrowIfContains<T>(this ISet<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            set.AddOrThrowIfContains(item);
        }
    }
}
