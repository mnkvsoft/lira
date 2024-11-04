using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions.Transform.Impl.Format;

internal static class FormattableHelper
{
    public static string FormatOrThrow(this object value, string format)
    {
        var formattable = GetFormattable(value);

        return formattable.ToString(format, null);
    }

    private static IFormattable GetFormattable(object value)
    {
        if (value is IFormattable formattable)
            return formattable;

        if (value is string valueStr)
        {
            var typed = valueStr.Typed();

            if (typed is IFormattable f)
                return f;
        }

        throw new Exception($"Cannot format {value.GetType()} with value '{value}'");
    }
}