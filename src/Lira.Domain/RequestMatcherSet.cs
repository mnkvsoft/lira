using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;

namespace Lira.Domain;

public class RequestMatcherSet
{
    private readonly MethodRequestMatcher? _methodMatcher;
    private readonly PathRequestMatcher? _pathMatcher;
    private readonly QueryStringRequestMatcher? _queryStringMatcher;
    private readonly HeadersRequestMatcher? _headersMatcher;
    private readonly BodyRequestMatcher? _bodyMatcher;

    public RequestMatcherSet(
        MethodRequestMatcher? methodMatcher, 
        PathRequestMatcher? pathMatcher, 
        QueryStringRequestMatcher? queryStringMatcher, 
        HeadersRequestMatcher? headersMatcher, 
        BodyRequestMatcher? bodyMatcher)
    {
        _methodMatcher = methodMatcher;
        _pathMatcher = pathMatcher;
        _queryStringMatcher = queryStringMatcher;
        _headersMatcher = headersMatcher;
        _bodyMatcher = bodyMatcher;
    }
    
    internal async Task<RuleMatchResult> IsMatch(RequestData request)
    {
        int weightMethod = 0;
        int weightPath = 0;
        int weightQuery = 0;
        int weightBody = 0;
        int weightHeaders = 0;

        RequestMatchResult matchResult;

        var matchedValuesSet = new Dictionary<string, string?>();

        if (_methodMatcher != null)
        {
            matchResult = await _methodMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            matchedValuesSet.Add(matched.MatchedValues);
            weightMethod = matched.Weight;
        }

        if (_pathMatcher != null)
        {
            matchResult = await _pathMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            matchedValuesSet.Add(matched.MatchedValues);
            weightPath = matched.Weight;
        }

        if (_queryStringMatcher != null)
        {
            matchResult = await _queryStringMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            matchedValuesSet.Add(matched.MatchedValues);
            weightQuery = matched.Weight;
        }

        if (_headersMatcher != null)
        {
            matchResult = await _headersMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            matchedValuesSet.Add(matched.MatchedValues);
            weightHeaders = matched.Weight;
        }

        if (_bodyMatcher != null)
        {
            matchResult = await _bodyMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            matchedValuesSet.Add(matched.MatchedValues);
            weightBody = matched.Weight;
        }

        return new RuleMatchResult.Matched(
            new RuleMatchWeight(
                weightMethod,
                weightPath,
                weightQuery,
                weightBody,
                weightHeaders),
            matchedValuesSet);
    }
}
