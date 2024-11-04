using Lira.Common.Exceptions;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

static class PatternPartsExtensions
{
    public record TwoPartsWithSecondRequired(PatternParts One, PatternParts Second);
    public record TwoPartsWithSecondOptional(PatternParts One, PatternParts? Second);

    public static TwoPartsWithSecondRequired Trim(this TwoPartsWithSecondRequired parts)
        => new TwoPartsWithSecondRequired(parts.One.Trim(), parts.Second.Trim());

    public static TwoPartsWithSecondOptional Trim(this TwoPartsWithSecondOptional parts)
        => new TwoPartsWithSecondOptional(parts.One.Trim(), parts.Second?.Trim());

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
        var result = new List<PatternParts>(15);

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
                        remainder.Add(new PatternPart.Static(s));
                    }
                    else
                    {
                        remainder.Add(new PatternPart.Static(s));
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
}