namespace SimpleMockServer.Domain.Matching.Request;

public abstract record TextPatternPart
{
    public abstract bool IsMatch(string? value);

    public record Static(string Expected) : TextPatternPart
    {
        public override bool IsMatch(string? current) => Expected.Equals(current, StringComparison.OrdinalIgnoreCase);
    }

    public record NullOrEmpty : TextPatternPart
    {
        public override bool IsMatch(string? current) => string.IsNullOrWhiteSpace(current);
    }

    public record Dynamic(string? Start, string? End, IStringMatchFunction MatchFunction) : TextPatternPart
    {
        public override bool IsMatch(string? current)
        {
            var toMatch = current;

            if (Start != null)
            {
                if (string.IsNullOrWhiteSpace(current))
                    return false;

                if (!current.StartsWith(Start, StringComparison.OrdinalIgnoreCase))
                    return false;

                toMatch = current.Substring(Start.Length);
            }


            if (End != null)
            {
                if (string.IsNullOrWhiteSpace(current))
                    return false;

                if (!current.EndsWith(End, StringComparison.OrdinalIgnoreCase))
                    return false;

                toMatch = toMatch!.Substring(0, toMatch.Length - End.Length);
            }

            return MatchFunction.IsMatch(toMatch);
        }
    }
}
