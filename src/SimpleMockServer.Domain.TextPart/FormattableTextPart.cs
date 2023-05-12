using System.Globalization;

namespace SimpleMockServer.Domain.TextPart;

public record FormattableTextPart(IObjectTextPart ObjectTextPart, string Format) : IObjectTextPart
{
    public object? Get(RequestData request) => ObjectTextPart.Get(request)?.FormatOrThrow(Format);
}

public record GlobalFormattableTextPart(IGlobalObjectTextPart GlobalObjectTextPart, string Format) : IGlobalObjectTextPart
{
    public object? Get(RequestData request) => GlobalObjectTextPart.Get(request)?.FormatOrThrow(Format);

    public object? Get() => GlobalObjectTextPart.Get()?.FormatOrThrow(Format);
}

internal static class FormattableHelper
{
    public static string FormatOrThrow(this object value, string format)
    {
        var formattable = GetFormattable(value);

        return formattable.ToString(format, null);
    }

    private static IFormattable GetFormattable(object value)
    {
        IFormattable formattable;

        if (value is IFormattable f)
            formattable = f;
        else if (value is string valueStr)
            formattable = GetFormattable(valueStr);
        else
            throw new Exception($"Cannot format {value.GetType()} with value '{value}'");
        
        return formattable;
    }

    private static IFormattable GetFormattable(string value)
    {
        if (long.TryParse(value, CultureInfo.InvariantCulture, out var longValue))
            return longValue;
        
        if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var decimalValue))
            return decimalValue;

        if (Guid.TryParse(value, CultureInfo.InvariantCulture, out var guidValue))
            return guidValue;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, out var dateValue))
            return dateValue;

        throw new Exception($"Cannot convert string value '{value}' to formattable");
    }
}
