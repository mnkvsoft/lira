namespace Lira.Domain.Matching.Request;

public abstract record TextPatternPart
{
    public abstract Task<bool> Match(RuleExecutingContext context, string? value);

    public record Static(string Expected) : TextPatternPart
    {
        public override Task<bool> Match(RuleExecutingContext context, string? current) => Task.FromResult(Expected.Equals(current, StringComparison.OrdinalIgnoreCase));
    }

    public record NullOrEmpty : TextPatternPart
    {
        public override Task<bool> Match(RuleExecutingContext context, string? current) => Task.FromResult(string.IsNullOrWhiteSpace(current));
    }

    public record Dynamic(string? Start, string? End, IMatchFunction MatchFunction) : TextPatternPart
    {
        public override async Task<bool> Match(RuleExecutingContext context, string? current)
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

            return await MatchFunction.IsMatch(context, toMatch);
        }
    }
}
