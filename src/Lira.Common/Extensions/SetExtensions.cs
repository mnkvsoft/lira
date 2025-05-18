namespace Lira.Common.Extensions;

public static class SetExtensions
{
    public static void AddOrThrowIfContains<T>(this ISet<T> set, T item)
    {
        if (!set.Add(item))
            throw new InvalidOperationException($"Item {item} already contains in set");
    }

    public static void AddRangeOrThrowIfContains<T>(this ISet<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            set.AddOrThrowIfContains(item);
        }
    }

    public static void TryAddRange<T>(this ISet<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            set.Add(item);
        }
    }

    public static bool ContainsAll<T>(this IReadOnlySet<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            if (!set.Contains(item))
                return false;
        }

        return true;
    }
}