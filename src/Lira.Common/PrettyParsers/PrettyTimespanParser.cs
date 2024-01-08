using Lira.Common.Extensions;

namespace Lira.Common.PrettyParsers;

public class PrettyTimespanParser : Interval<TimeSpan>.IConverter
{
    private static readonly List<(string[], Func<int, TimeSpan>)> NameToCreatorMap = new()
    {
        (new[]{ "ms", "milliseconds", "millisecond" }, value => TimeSpan.FromMilliseconds(value)),
        (new[]{ "s",  "second",       "seconds" }, value => TimeSpan.FromSeconds(value)),
        (new[]{ "m",  "minute",       "minutes" }, value => TimeSpan.FromMinutes(value)),
        (new[]{ "h",  "hour",         "hours" }, value => TimeSpan.FromHours(value)),
        (new[]{ "d",  "day",          "days" }, value => TimeSpan.FromDays(value)),
    };

    public static TimeSpan Parse(string? str)
    {
        if (!TryParse(str, out var result))
            throw new ArgumentException($"Invalid timespan value: '{str}'");
        return result;
    }
    
    public static bool TryParse(string? str, out TimeSpan result)
    {
        result = default;
        
        if (string.IsNullOrWhiteSpace(str))
            return false;

        var (countStr, unit) = str.SplitToTwoParts(" ").Trim();
        countStr = countStr.Replace("_", "");

        if (!int.TryParse(countStr, out var count))
            return TimeSpan.TryParse(str, out result);

        if (unit == null)
        {
            result = TimeSpan.FromMilliseconds(count);
            return true;
        }

        foreach (var pair in NameToCreatorMap)
        {
            var names = pair.Item1;
            var create = pair.Item2;

            if (IsMatch(unit, names))
            {
                result = create(count);
                return true;
            }
        }

        return false;
    }

    private static bool IsMatch(string str, string[] values)
    {
        foreach (var value in values)
        {
            if (str.Equals(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public bool TryConvert(string str, out TimeSpan result) => TryParse(str, out result);
}
