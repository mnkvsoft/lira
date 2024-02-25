namespace Lira.Domain.Extensions;
internal static class DictionaryExtensions
{
    public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue> other)
    {
        foreach(var kvp in other)
        {
            dictionary.Add(kvp.Key, kvp.Value);
        }
    }
}
