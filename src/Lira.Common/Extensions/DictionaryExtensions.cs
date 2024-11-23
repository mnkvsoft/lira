namespace Lira.Common.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, object> dictionary,
        TKey key,
        Func<TValue> factory)
        where TValue : notnull
    {
        if (dictionary.TryGetValue(key, out var result))
            return (TValue)result;

        result = factory();
        dictionary[key] = result;
        return (TValue)result;
    }
}