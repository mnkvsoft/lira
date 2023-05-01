namespace SimpleMockServer.Domain.Matching.Request.Matchers.QueryString;

public class QueryStringRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyDictionary<string, TextPatternPart> _queryParamToPatternMap;

    public QueryStringRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> queryParamToPatternMap)
    {
        _queryParamToPatternMap = queryParamToPatternMap;
    }

    public Task<bool> IsMatch(RequestData request)
    {
        foreach (var pair in _queryParamToPatternMap)
        {
            var parName = pair.Key;
            if (!request.Query.TryGetValue(parName, out var value))
                return Task.FromResult(false);

            var pattern = pair.Value;
            if (!pattern.IsMatch(value))
                return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
}
