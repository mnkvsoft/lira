using System.Diagnostics.CodeAnalysis;
using SimpleMockServer.Common.Extensions;

namespace SimpleMockServer.Common;

public record Interval<T> where T : struct, IComparable<T>
{
    public T From { get; }
    public T To { get; }

    public Interval(T from, T to)
    {
        if (IsValid(from, to))
            throw new ArgumentOutOfRangeException($"Argument 'from' ({from}) must be less or equal than argument 'to' ({to})");

        From = from;
        To = to;
    }

    public override string ToString() => $"[{From} - {To}]";
    public bool InRange(T value) => From.CompareTo(value) <= 0 && (value.CompareTo(To) <= 0);
    public bool IsIntersect(Interval<T> interval)
    {
        return interval.From.CompareTo(To) <= 0 || From.CompareTo(interval.To) >= 0;
    }

    public interface IConverter
    {
        bool TryConvert(string str, out T result);
    }
    
    private class ConverterAdapter : IConverter
    {
        public bool TryConvert(string str, out T result)
        {
            return StringConverter<T>.TryConvert(str, out result);
        }
    }

    public static Interval<T> Parse(string str, IConverter converter)
    {
        if (!TryParse(str, out var result, converter))
            throw new Exception($"Cannot parse interval value '{str}'");
        return result;
    }

    public static bool TryParse(string str, [MaybeNullWhen(false)] out Interval<T> result)
    {
        return TryParse(str, out result, new ConverterAdapter());
    }
    
    public static bool TryParse(string str, [MaybeNullWhen(false)] out Interval<T> result, IConverter converter)
    {
        result = null;

        str = str.TrimStart('[').TrimEnd(']');
        var (fromStr, toStr) = str.SplitToTwoParts("-").Trim();
        if (toStr == null)
            return false;

        if (!converter.TryConvert(fromStr, out var from))
            return false;
        
        if (!converter.TryConvert(toStr, out var to))
            return false;
        
        result = new Interval<T>(from, to);
        return true;
    }
    
    private static bool IsValid(T from, T to)
    {
        return from.CompareTo(to) >= 0;
    }
}
