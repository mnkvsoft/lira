namespace Lira.Common.Extensions;

public static class ReadOnlyCollectionExtensions
{
    public static int GetMaxIndex<T>(this IReadOnlyCollection<T> items)
    {
        return items.Count - 1;
    }

    public static IReadOnlyCollection<T> NewWith<T>(this IReadOnlyCollection<T> items, params T[] newItems)
    {
        var newCollection = new List<T>(items.Count + newItems.Length);
        newCollection.AddRange(items);
        newCollection.AddRange(newItems);
        return newCollection;
    }

    public static T Random<T>(this IReadOnlyList<T> list)
    {
        return list[System.Random.Shared.Next(0, list.Count)];
    }
}
