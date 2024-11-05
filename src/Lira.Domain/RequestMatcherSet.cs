namespace Lira.Domain;

public class RequestMatcherSet
{
    private readonly IReadOnlyCollection<IRequestMatcher> _matchers;

    public RequestMatcherSet(IReadOnlyCollection<IRequestMatcher> matchers)
    {
        _matchers = matchers;
    }

    internal async Task<RuleMatchResult> IsMatch(RuleExecutingContext context)
    {
        var matcheds = new List<Matched>();

        foreach (var matcher in _matchers)
        {
            var matchResult = await matcher.IsMatch(context);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            context.AddMatchedValue(matched.MatchedValues);
            matcheds.Add(matched);
        }

        return new RuleMatchResult.Matched(new RuleMatchWeight(matcheds));
    }
}
