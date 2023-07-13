namespace SimpleMockServer.Domain.Configuration;

internal static class PrettyInt64Parser
{
    private static readonly List<(char, string)> NameToZerosMap = new()
    {
        ('k', "000"),
        ('m', "000000"),
        ('g', "000000000"),
        ('t', "000000000000")
    };

    public static bool TryParse(string? str, out long result)
    {
        result = default;
        
        if (string.IsNullOrWhiteSpace(str))
            return false;

        str = str.Replace("_", "").Trim();
        
        if (long.TryParse(str, out result))
            return true;

        char unit = str.Last();
        
        foreach (var pair in NameToZerosMap)
        {
            var name = pair.Item1;
            var zeros = pair.Item2;

            if (char.ToUpperInvariant(unit) == char.ToUpperInvariant(name))
            {
                return long.TryParse(str[..^1] + zeros, out result);
            }
        }

        return false;
    }
}
