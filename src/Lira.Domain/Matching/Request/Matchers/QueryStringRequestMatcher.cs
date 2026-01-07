namespace Lira.Domain.Matching.Request.Matchers;

public class QueryStringRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyDictionary<string, TextPatternPart> _queryParamToPatternMap;

    public QueryStringRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> queryParamToPatternMap)
    {
        _queryParamToPatternMap = queryParamToPatternMap;
    }

    async Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext ctx)
    {
        int weight = 0;

        foreach (var pair in _queryParamToPatternMap)
        {
            var parName = pair.Key;
            if (!ctx.RequestData.Query.TryGetValue(parName, out var value))
                return RequestMatchResult.NotMatched;

            var pattern = pair.Value;
            if (await pattern.Match(ctx, value) == false)
                return RequestMatchResult.NotMatched;

            weight += TextPatternPartWeightCalculator.Calculate(pattern);
        }
        return RequestMatchResult.Matched(name: "query", weight);
    }
}
