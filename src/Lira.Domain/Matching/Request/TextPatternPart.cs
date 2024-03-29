namespace Lira.Domain.Matching.Request;

public abstract record TextPatternPart
{
    public abstract record MatchResult
    {
        public record NotMatch : MatchResult
        {
            public static readonly NotMatch Instance = new NotMatch();
        }

        public record Matched(string? DynamicValueId, string? DynamicValue) : MatchResult
        {
            public static readonly Matched WithoutDynamic = new(null, null);
        }
    }

    public abstract MatchResult Match(string? value);

    public record Static(string Expected) : TextPatternPart
    {
        public override MatchResult Match(string? current)
        {
            return Expected.Equals(current, StringComparison.OrdinalIgnoreCase)
                ? MatchResult.Matched.WithoutDynamic
                : MatchResult.NotMatch.Instance;

        }
    }

    public record NullOrEmpty : TextPatternPart
    {
        public override MatchResult Match(string? current)
        {
            return string.IsNullOrWhiteSpace(current)
               ? MatchResult.Matched.WithoutDynamic
               : MatchResult.NotMatch.Instance;
        }
    }

    public record Dynamic(string? Start, string? End, IMatchFunction MatchFunction, string? ValueId) : TextPatternPart
    {
        public override MatchResult Match(string? current)
        {
            MatchResult.NotMatch notMatch = MatchResult.NotMatch.Instance;
            var toMatch = current;

            if (Start != null)
            {
                if (string.IsNullOrWhiteSpace(current))
                    return notMatch;

                if (!current.StartsWith(Start, StringComparison.OrdinalIgnoreCase))
                    return notMatch;

                toMatch = current.Substring(Start.Length);
            }

            if (End != null)
            {
                if (string.IsNullOrWhiteSpace(current))
                    return notMatch;

                if (!current.EndsWith(End, StringComparison.OrdinalIgnoreCase))
                    return notMatch;

                toMatch = toMatch!.Substring(0, toMatch.Length - End.Length);
            }

            return MatchFunction.IsMatch(toMatch)
               ? new MatchResult.Matched(ValueId, toMatch)
               : notMatch;
        }
    }
}
