namespace Lira.Domain.Matching.Request.Matchers;

public class QueryStringRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyDictionary<string, TextPatternPart> _queryParamToPatternMap;

    public QueryStringRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> queryParamToPatternMap)
    {
        _queryParamToPatternMap = queryParamToPatternMap;
    }

    public Task<RequestMatchResult> IsMatch(RequestData request)
    {
        int weight = 0;
        
        foreach (var pair in _queryParamToPatternMap)
        {
            var parName = pair.Key;
            if (!request.Query.TryGetValue(parName, out var value))
                return Task.FromResult(RequestMatchResult.NotMatched);

            var pattern = pair.Value;
            if (!pattern.IsMatch(value))
                return Task.FromResult(RequestMatchResult.NotMatched);

            weight += TextPatternPartWeightCalculator.Calculate(pattern);
        }
        return Task.FromResult(RequestMatchResult.Matched(weight));
    }
}
