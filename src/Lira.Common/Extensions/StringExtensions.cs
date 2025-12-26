using System.Globalization;

namespace Lira.Common.Extensions;
public static class StringExtensions
{
    public static string Join(this string value, char c) => string.Join(c, value);

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

    public static (string part1, string? part2) SplitToTwoParts(this string value, char splitter) => SplitToTwoParts(value, splitter.ToString());
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
        var replacedNl = value.Replace("\n", nl);
        return
            "============== begin ==============" + nl + nl +
            replacedNl + nl + nl +
            "=============== end ===============" + nl + nl;
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

    public static List<(string Name, int Index)> GetPositions(this string str, IReadOnlyCollection<string> names)
    {
        string current = str;
        var result = new List<(string Name, int Index)>();
        int deletedLength = 0;

        while (current.Length > 0)
        {
            int nearIndex = current.Length - 1;
            string? nearName = null;
            foreach (var name in names)
            {
                var idx = current.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                if (idx > 0 && idx < nearIndex)
                {
                    nearIndex = idx;
                    nearName = name;
                }
            }

            if (nearName != null)
            {
                result.Add((nearName, nearIndex + deletedLength));
                var newCurrent = current.Substring(nearIndex + nearName.Length);
                deletedLength += (current.Length - newCurrent.Length);
                current = newCurrent;
            }
            else
            {
                return result;
            }
        }

        return result;
    }

    // public static IEnumerable<string> AlignIndents(this IReadOnlyCollection<string> lines, int count = 0)
    // {
    //     var minIndent = lines.Min(GetCountWhitespacesStart);
    //     var indent = new string(' ', count);
    //
    //     foreach (var line in lines)
    //     {
    //         yield return indent + new string(' ', GetCountWhitespacesStart(line) - minIndent) + line.TrimStart();
    //     }
    // }

    public static int GetCountWhitespacesStart(this string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] != ' ')
                return i;
        }

        return str.Length;
    }

    public static int GetCountWhitespacesEnd(this string str)
    {
        for (int i = str.Length - 1; i >= 0; i--)
        {
            if (str[i] == '\n')
                return str.Length - 1 - i;
        }

        return str.Length;
    }

    public static IEnumerable<string> TrimIfSingleLine(this IEnumerable<string> lines) => ActionIfSingleLine(lines, str => str.Trim());
    public static IEnumerable<string> TrimEndIfSingleLine(this IEnumerable<string> lines) => ActionIfSingleLine(lines, str => str.TrimEnd());

    private static IEnumerable<string> ActionIfSingleLine(this IEnumerable<string> lines, Func<string, string> action)
    {
        int count = 0;
        string? firstSourceLine = null;
        foreach (var line in lines)
        {
            if (count == 0)
            {
                firstSourceLine = line;
            }
            else if (count == 1)
            {
                yield return firstSourceLine!;
                yield return line;
            }
            else
            {
                yield return line;
            }

            count++;
        }

        if(count == 1)
            yield return action(firstSourceLine!);
    }

    public static IEnumerable<string> TrimEmptyLines(this IEnumerable<string> lines) => lines.TrimStartEmptyLines().TrimEndEmptyLines();

    public static IEnumerable<string> TrimEndEmptyLines(this IEnumerable<string> lines)
    {
        return TrimStartEmptyLines(lines.Reverse()).Reverse();
    }

    public static IEnumerable<string> TrimStartEmptyLines(this IEnumerable<string> lines)
    {
        bool start = true;

        foreach (var line in lines)
        {
            if (start && string.IsNullOrWhiteSpace(line))
                continue;
            if (start)
                start = false;
            yield return line;
        }
    }
}

public record SplitResult(string Splitter, string LeftPart, string RightPart);
