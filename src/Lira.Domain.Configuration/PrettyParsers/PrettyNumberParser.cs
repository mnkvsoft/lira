using Lira.Common;

namespace Lira.Domain.Configuration.PrettyParsers;

internal class PrettyNumberParser<TNumber> : Interval<TNumber>.IConverter 
    where TNumber : struct, IComparable<TNumber>
{
    private static readonly List<(char, decimal)> NameToMultiplierMap = new()
    {
        ('k', 1000),
        ('m', 1000000),
        ('g', 1000000000),
        ('t', 1000000000000)
    };

    public static bool TryParse(string? str, out TNumber result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(str))
            return false;

        str = str.Replace("_", "").Trim();

        if (StringConverter<TNumber>.TryConvert(str, out result))
            return true;

        char unit = str.Last();

        foreach (var pair in NameToMultiplierMap)
        {
            var name = pair.Item1;
            var multiplier  = pair.Item2;

            if (char.ToUpperInvariant(unit) == char.ToUpperInvariant(name))
            {
                if (!StringConverter<TNumber>.TryConvert(str[..^1], out var valueInUnit))
                    return false;

                result = (TNumber)((dynamic)valueInUnit * multiplier);
                return true;
            }
        }

        return false;
    }

    public bool TryConvert(string str, out TNumber result) => TryParse(str, out result);
}
