using System.Globalization;

namespace Lira.Common.Extensions;
public static class StringExtensions
{
    public static string TrimStart(this string value, string trimString)
    {
        if (string.IsNullOrEmpty(trimString))
            return value;

        string result = value;
        while (result.StartsWith(trimString))
        {
            result = result.Substring(trimString.Length);
        }

        return result;
    }

    public static string TrimEnd(this string value, string trimString)
    {
        if (string.IsNullOrEmpty(trimString))
            return value;

        string result = value;
        while (result.EndsWith(trimString))
        {
            result = result.Substring(0, result.Length - trimString.Length);
        }

        return result;
    }

    public static (string part1, string? part2) SplitToTwoParts(this string value, string splitter)
    {
        var index = value.IndexOf(splitter, StringComparison.Ordinal);

        if (index < 0)
            return (value, null);

        string part1= value.Substring(0, length: index);
        string part2 = value.Substring(index + splitter.Length);

        return (part1, part2);
    }

    public static (string part1, string? part2) SplitToTwoPartsFromEnd(this string value, string splitter)
    {
        var index = value.LastIndexOf(splitter, StringComparison.Ordinal);

        if (index < 0)
            return (value, null);

        string part1= value.Substring(0, length: index);
        string part2 = value.Substring(index + splitter.Length);

        return (part1, part2);
    }

    public static (string part1, string? part2) Trim(this (string part1, string? part2) value)
    {
        return (value.part1.Trim(), value.part2?.Trim());
    }

    public static string WrapBeginEnd(this string value)
    {
        var nl = Environment.NewLine;
        return
            "========== begin ==========" + nl + nl +
            value + nl + nl +
            "=========== end ===========" + nl + nl;
    }

    public static object Typed(this string value)
    {
        if (bool.TryParse(value, out var boolValue))
            return boolValue;

        if (long.TryParse(value, CultureInfo.InvariantCulture, out var longValue))
            return longValue;

        if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var decimalValue))
            return decimalValue;

        if (Guid.TryParse(value, CultureInfo.InvariantCulture, out var guidValue))
            return guidValue;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, out var dateValue))
            return dateValue;

        return value;
    }

    public static TwoPartsRequired SplitToTwoPartsRequired(this string value, string splitter)
    {
        var index = value.IndexOf(splitter, StringComparison.Ordinal);

        if (index < 0)
            throw new Exception($"Value must contains splitter '{splitter}'. Value: '{value}'");

        string part1= value.Substring(0, length: index);
        string part2 = value.Substring(index + splitter.Length);

        return new TwoPartsRequired(part1, part2);
    }

    public record TwoPartsRequired(string First, string Second);

    public static TwoPartsRequired Trim(this TwoPartsRequired value)
    {
        return new TwoPartsRequired (value.First.Trim(), value.Second.Trim());
    }

    public static SplitResult? SplitBy(this string str, params string[] splitters)
    {
        foreach(string splitter in splitters)
        {
            if(str.Contains(splitter, StringComparison.OrdinalIgnoreCase))
            {
                (string left, string right) = str.SplitToTwoPartsRequired(splitter);
                return new SplitResult(splitter, left, right);
            }
        }
        return null;
    }

    public static SplitResult Trim(this SplitResult splitResult)
    {
        return new SplitResult(splitResult.Splitter, splitResult.LeftPart.Trim(), splitResult.RightPart.Trim());
    }
}

public record SplitResult(string Splitter, string LeftPart, string RightPart);
