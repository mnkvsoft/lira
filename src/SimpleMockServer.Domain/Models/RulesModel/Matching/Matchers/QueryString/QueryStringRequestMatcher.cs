using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.QueryString;

public class QueryStringRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyDictionary<string, ValuePattern> _queryParamToPatternMap;

    public QueryStringRequestMatcher(IReadOnlyDictionary<string, ValuePattern> queryParamToPatternMap)
    {
        _queryParamToPatternMap = queryParamToPatternMap;
    }

    public Task<bool> IsMatch(HttpRequest request)
    {
        foreach (var pair in _queryParamToPatternMap)
        {
            string parName = pair.Key;
            if (!request.Query.TryGetValue(parName, out var value))
                return Task.FromResult(false);

            var pattern = pair.Value;
            if (!pattern.IsMatch(value))
                return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
}
