namespace Lira.Common.Extensions;
public static class DictionaryExtensions
{
    public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue> other)
    {
        foreach(var kvp in other)
        {
            dictionary.Add(kvp.Key, kvp.Value);
        }
    }

    public static TValue TryGetValueOrCreate<TKey, TValue>(this IDictionary<TKey, object> dictionary, TKey key, Func<TValue> factory) where TValue: notnull
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = factory();
            dictionary.Add(key, value);
            return (TValue)value;
        }
        return (TValue)value;
    }
}
