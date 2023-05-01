using SimpleMockServer.Common.Extensions;

namespace SimpleMockServer.Domain.Configuration;

internal static class PrettyTimespanParser
{
    private static List<(string[], Func<int, TimeSpan>)> _nameToCreatorMap = new List<(string[], Func<int, TimeSpan>)>
    {
        {(new string[]{ "ms", "milliseconds", "millisecond" }, value => TimeSpan.FromMilliseconds(value))},
        {(new string[]{  "s", "second", "seconds" }, value => TimeSpan.FromSeconds(value))},
        {(new string[]{ "m", "minute", "minutes" }, value => TimeSpan.FromMinutes(value))},
        {(new string[]{ "h", "hour", "hours" }, value => TimeSpan.FromHours(value))},
        {(new string[]{ "d", "day", "days" }, value => TimeSpan.FromDays(value))},
    };

    public static TimeSpan Parse(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException($"Invalid timespan value: '{str}'");

        (var countStr, var unit) = str.SplitToTwoParts(" ").Trim();
        countStr = countStr.Replace("_", "");

        if (!int.TryParse(countStr, out var count))
            return TimeSpan.Parse(str);

        if (unit == null)
            return TimeSpan.FromMilliseconds(count);

        foreach (var pair in _nameToCreatorMap)
        {
            var names = pair.Item1;
            var create = pair.Item2;

            if (IsMatch(unit, names))
                return create(count);
        }

        throw new ArgumentException($"Invalid timespan value: '{str}'");
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
}
