namespace SimpleMockServer.Domain.Matching;

interface IMatchFunction
{
    bool IsMatch(string value);
}

record DynamicSimpleValuePattern(string? Start, string? End, IMatchFunction MatchFunction) : IValuePattern
{
    public bool IsMatch(string current)
    {
        string toMatch = current;

        if (Start != null)
        {
            if (!current.StartsWith(Start, StringComparison.OrdinalIgnoreCase))
                return false;

            toMatch = current.Substring(Start.Length);
        }


        if (End != null)
        {
            if (!current.EndsWith(End, StringComparison.OrdinalIgnoreCase))
                return false;

            toMatch = toMatch.Substring(0, toMatch.Length - End.Length);
        }

        return MatchFunction.IsMatch(toMatch);
    }
}