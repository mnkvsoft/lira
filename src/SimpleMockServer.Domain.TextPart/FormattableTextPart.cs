namespace SimpleMockServer.Domain.TextPart;

// public interface IFormattableTextPart
// {
//     string? Format { get; }
//     IObjectTextPart ObjectTextPart { get; }
// }

public record FormattableTextPart(IObjectTextPart ObjectTextPart, string Format) : IObjectTextPart
{
    public object? Get(RequestData request) => ObjectTextPart.Get(request)?.FormatIfFormattable(Format);
}

public record GlobalFormattableTextPart(IGlobalObjectTextPart GlobalObjectTextPart, string Format) : IGlobalObjectTextPart
{
    public object? Get(RequestData request) => GlobalObjectTextPart.Get(request)?.FormatIfFormattable(Format);

    public object? Get() => GlobalObjectTextPart.Get()?.FormatIfFormattable(Format);
}

internal static class FormattableHelper
{
    public static string FormatIfFormattable(this object value, string format)
    {
        if (value is IFormattable formattable)
        {
            return formattable.ToString(format, null);
        }

        return value.ToString() ?? "";
    }
}
