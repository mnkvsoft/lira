namespace Lira.Domain.Matching.Request.Matchers;

public class QueryStringRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyDictionary<string, TextPatternPart> _queryParamToPatternMap;

    public QueryStringRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> queryParamToPatternMap)
    {
        _queryParamToPatternMap = queryParamToPatternMap;
    }

    internal Task<RequestMatchResult> IsMatch(RequestData request)
    {
        var matchedValuesSet = new Dictionary<string, string?>();
        int weight = 0;
        
        foreach (var pair in _queryParamToPatternMap)
        {
            var parName = pair.Key;
            if (!request.Query.TryGetValue(parName, out var value))
                return Task.FromResult(RequestMatchResult.NotMatched);

            var pattern = pair.Value;
            if (pattern.Match(value) is not TextPatternPart.MatchResult.Matched matched)
                return Task.FromResult(RequestMatchResult.NotMatched);

            matchedValuesSet.AddIfValueIdNotNull(matched);

            weight += TextPatternPartWeightCalculator.Calculate(pattern);
        }
        return Task.FromResult(RequestMatchResult.Matched(weight, matchedValuesSet));
    }
}
