namespace Lira.Domain.Matching.Request.Matchers;

public class QueryStringRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyDictionary<string, TextPatternPart> _queryParamToPatternMap;

    public QueryStringRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> queryParamToPatternMap)
    {
        _queryParamToPatternMap = queryParamToPatternMap;
    }

    async Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext context)
    {
        int weight = 0;

        foreach (var pair in _queryParamToPatternMap)
        {
            var parName = pair.Key;
            if (!context.RequestData.Query.TryGetValue(parName, out var value))
                return RequestMatchResult.NotMatched;

            var pattern = pair.Value;
            if (await pattern.Match(context, value) == false)
                return RequestMatchResult.NotMatched;

            weight += TextPatternPartWeightCalculator.Calculate(pattern);
        }
        return RequestMatchResult.Matched(name: "query", weight);
    }
}
