namespace SimpleMockServer.Common.Extensions;
public static class StringExtensions
{
    public static Stream ToStream(this string s)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    public static bool EqualsIgnoreCase(this string s, string str)
    {
        return s.Equals(str, StringComparison.OrdinalIgnoreCase);
    }

    public static string? GetNullOrValue(this string? value, Func<string, string> getValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return getValue(value);
    }

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

    public static (string part1, string? part2) Trim(this (string part1, string? part2) value)
    {
        return (value.part1.Trim(), value.part2?.Trim());
    }
    
    public static (string part1, string part2) SplitToTwoPartsRequired(this string value, string splitter)
    {
        var index = value.IndexOf(splitter, StringComparison.Ordinal);

        if (index < 0)
            throw new Exception($"Value must contains splitter '{splitter}'. Value: '{value}'");

        string part1= value.Substring(0, length: index);
        string part2 = value.Substring(index + splitter.Length);

        return (part1, part2);
    }

    public static (string part1, string part2) TrimRequired(this (string part1, string part2) value)
    {
        return (value.part1.Trim(), value.part2.Trim());
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
