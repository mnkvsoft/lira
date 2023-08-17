using System.ComponentModel;

namespace SimpleMockServer.Common;

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
