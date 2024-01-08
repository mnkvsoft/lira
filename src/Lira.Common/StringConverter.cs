using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Lira.Common;

public static class StringConverter<T> where T : struct
{
    private static readonly TypeConverter Converter = TypeDescriptor.GetConverter(typeof(T));
        
    public static bool TryConvert(string str, out T result)
    {
        result = default;
        if (!Converter.IsValid(str))
            return false;
        
        object? fromObj = Converter.ConvertFromInvariantString(str);
        if (fromObj == null)
            return false;

        result = (T)fromObj;
        return true;
    }
    
    public static T Convert(string str)
    {
        if (!TryConvert(str, out var result))
            throw new FormatException($"Cannot convert string '{str}' to type {typeof(T)}");

        return result;
    }
}

public static class StringConverter
{
    public static bool TryConvert(Type type, string str, [MaybeNullWhen(false)] out object result)
    {
        result = null;
        TypeConverter converter = TypeDescriptor.GetConverter(type);
        
        if (!converter.IsValid(str))
            return false;
        
        result = converter.ConvertFromInvariantString(str);
        if (result == null)
            return false;

        return true;
    }
    
    public static object Convert(Type type, string str)
    {
        if (!TryConvert(type, str, out object? result))
            throw new Exception($"String '{str}' cannot be convert to type: '{type}'. String: '{str}'");
        return result;
    }
}
