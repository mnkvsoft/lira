using Lira.Common.Exceptions;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

static class PatternPartsExtensions
{
    public record TwoPartsWithSecondRequired(PatternParts One, PatternParts Second);
    public record TwoPartsWithSecondOptional(PatternParts One, PatternParts? Second);

    public static TwoPartsWithSecondRequired Trim(this TwoPartsWithSecondRequired parts) =>
        new(parts.One.Trim(), parts.Second.Trim());

    public static TwoPartsWithSecondOptional Trim(this TwoPartsWithSecondOptional parts) =>
        new(parts.One.Trim(), parts.Second?.Trim());

    public static PatternParts Trim(this PatternParts parts) => parts.TrimStart().TrimEnd();

    public static PatternParts TrimEnd(this PatternParts parts)
    {
        var temp = new List<PatternPart>(parts);
        temp.Reverse();

        var reversed = new PatternParts(temp);
        reversed = reversed.TrimStart();

        temp = new List<PatternPart>(reversed);
        temp.Reverse();

        return new PatternParts(temp);
    }

    public static PatternParts TrimStart(this PatternParts parts)
    {
        bool start = true;
        var temp = new List<PatternPart>(parts.Count);

        foreach (var part in parts)
        {
            if (start && part is PatternPart.Static st)
            {
                if(string.IsNullOrWhiteSpace(st.Value))
                    continue;

                temp.Add(new PatternPart.Static(st.Value.TrimStart()));
                start = false;
                continue;
            }

            start = false;
            temp.Add(part);
        }

        return new PatternParts(temp);
    }

    public static bool ContainsDynamic(this PatternParts parts)
        => parts.Any(x => x is PatternPart.Dynamic);

    public static bool ContainsInStatic(this PatternParts parts, string value)
        => parts.Any(x => x is PatternPart.Static st && st.Value.Contains(value));
    public static IReadOnlyCollection<PatternParts> GetLines(this PatternParts parts)
        => parts.Split("\n");
    public static PatternPart.Dynamic GetSingleDynamic(this PatternParts parts)
    {
        if(parts.Count != 1)
            throw new Exception($"Expecting only one dynamic block. Current count: {parts.Count}. Value: {parts} ");

        var part = parts.Single();

        if(part is not PatternPart.Dynamic dyn)
            throw new Exception($"Expecting only one dynamic block. Current block is static. Value: {parts} ");

        return dyn;
    }

    public static string GetStaticValue(this PatternParts parts) => GetSingleStatic(parts).Value;

    public static PatternPart.Static GetSingleStatic(this PatternParts parts)
    {
        if(parts.Count != 1)
            throw new Exception($"Expecting only one static block. Current count: {parts.Count}. Value: {parts} ");

        var part = parts.Single();

        if(part is not PatternPart.Static st)
            throw new Exception($"Expecting only one static block. Current block is dynamic. Value: {parts} ");

        return st;
    }

    public static PatternParts Replace(this PatternParts parts, PatternPart currentValue, PatternPart newValue)
    {
        var result = new List<PatternPart>();

        foreach (var part in parts)
        {
            if (part == currentValue)
            {
                result.Add(newValue);
            }
            else
            {
                result.Add(part);
            }

        }

        return new PatternParts(result);
    }

    public static PatternParts Replace(this PatternParts parts, Predicate<PatternPart> predicate, Func<PatternPart, PatternPart> getNewValue)
    {
        var result = new List<PatternPart>();
        foreach (var part in parts)
        {
            if (predicate(part))
            {
                result.Add(getNewValue(part));
                continue;
            }
            result.Add(part);
        }

        return new PatternParts(result);
    }

    public static string SingleStaticValueToString(this PatternParts parts)
    {
        return ((PatternPart.Static)parts.Single()).Value;
    }

    public static TwoPartsWithSecondOptional SplitToTwoParts(this PatternParts parts, string splitter)
    {
        var splitted = parts.Split(splitter);

        if (splitted.Count == 1)
            return new TwoPartsWithSecondOptional(splitted.First(), null);

        if (splitted.Count == 2)
            return new TwoPartsWithSecondOptional(splitted.First(), splitted.Last());

        var secondPart = new List<PatternPart>();
        foreach (var s in splitted.Skip(1))
        {
            secondPart.AddRange(s);
        }

        return new TwoPartsWithSecondOptional(splitted.First(), new PatternParts(secondPart));
    }

    public static TwoPartsWithSecondRequired SplitToTwoPartsRequired(this PatternParts parts, string splitter)
    {
        var splitted = parts.Split(splitter);

        if (splitted.Count == 1)
            throw new Exception($"'{parts}' not contains required splitter '{splitter}'");

        if (splitted.Count == 2)
            return new TwoPartsWithSecondRequired(splitted.First(), splitted.Last());

        var secondPart = new List<PatternPart>();
        foreach (var s in splitted.Skip(1))
        {
            secondPart.AddRange(s);
        }

        return new TwoPartsWithSecondRequired(splitted.First(), new PatternParts(secondPart));
    }

    public static IReadOnlyList<PatternParts> Split(this PatternParts pathParts, string splitter)
    {
        var result = new List<PatternParts>(50);

        List<PatternPart> remainder = new();
        foreach (PatternPart patternPart in pathParts)
        {
            if (patternPart is PatternPart.Static @static)
            {
                var splitted = @static.Value.Split(splitter);

                if (splitted.Length == 1)
                {
                    remainder.Add(@static);
                    continue;
                }

                for (int i = 0; i < splitted.Length; i++)
                {
                    string s = splitted[i];

                    if (i == splitted.Length - 1)
                    {
                        remainder.AddStaticIfNotEmpty(s);
                    }
                    else
                    {
                        remainder.AddStaticIfNotEmpty(s);
                        result.Add(new PatternParts(remainder));
                        remainder = new List<PatternPart>();
                    }
                }
            }
            else if (patternPart is PatternPart.Dynamic dynamic)
            {
                remainder.Add(dynamic);
            }
            else
            {
                throw new UnsupportedInstanceType(patternPart);
            }
        }

        if (remainder.Count != 0)
            result.Add(new PatternParts(remainder));

        return result;
    }

    public static void AddStaticIfNotEmpty(this IList<PatternPart> parts, string value)
    {
        if(value != string.Empty)
            parts.Add(new PatternPart.Static(value));
    }

    public static IEnumerable<PatternParts> TrimEmptyLines(this IEnumerable<PatternParts> lines) => lines.TrimStartEmptyLines().TrimEndEmptyLines();

    public static IEnumerable<PatternParts> TrimEndEmptyLines(this IEnumerable<PatternParts> lines)
    {
        return TrimStartEmptyLines(lines.Reverse()).Reverse();
    }

    public static IEnumerable<PatternParts> TrimStartEmptyLines(this IEnumerable<PatternParts> parts)
    {
        bool start = true;

        foreach (var part in parts)
        {
            if (start && part.IsNullOrWhiteSpace())
                continue;
            if (start)
                start = false;
            yield return part;
        }
    }

    private static bool IsNullOrWhiteSpace(this PatternParts parts)
    {
        return parts.All(x => x is PatternPart.Static st && string.IsNullOrWhiteSpace(st.Value));
    }

    public static PatternParts JoinWithNewLine(this IEnumerable<PatternParts> partsList) => Join(partsList, "\n");

    private static PatternParts Join(this IEnumerable<PatternParts> partsList, string separator)
    {
        var result = new List<PatternPart>();

        bool first = true;
        foreach (var parts in partsList)
        {
            if(!first)
                result.Add(new PatternPart.Static(separator));

            result.AddRange(parts);

            first = false;
        }

        return new PatternParts(result);
    }

    public static IEnumerable<PatternParts> TrimIfSingleLine(this IEnumerable<PatternParts> lines) => ActionIfSingleLine(lines, str => str.Trim());
    public static IEnumerable<PatternParts> TrimEndIfSingleLine(this IEnumerable<PatternParts> lines) => ActionIfSingleLine(lines, str => str.TrimEnd());

    private static IEnumerable<PatternParts> ActionIfSingleLine(this IEnumerable<PatternParts> lines, Func<PatternParts, PatternParts> action)
    {
        int count = 0;
        PatternParts? firstSourceLine = null;
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
}