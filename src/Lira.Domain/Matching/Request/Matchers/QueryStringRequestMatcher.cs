namespace Lira.Domain.Matching.Request.Matchers;

public class QueryStringRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> queryParamToPatternMap) : IRequestMatcher
{
    Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext context)
    {
        int weight = 0;

        foreach (var pair in queryParamToPatternMap)
        {
            var parName = pair.Key;
            if (!context.RequestContext.RequestData.Query.TryGetValue(parName, out var value))
                return Task.FromResult(RequestMatchResult.NotMatched);

            var pattern = pair.Value;
            if (pattern.Match(context, value) is not TextPatternPart.MatchResult.Matched)
                return Task.FromResult(RequestMatchResult.NotMatched);

            weight += TextPatternPartWeightCalculator.Calculate(pattern);
        }
        return Task.FromResult(RequestMatchResult.Matched(name: "query", weight));
    }
}
