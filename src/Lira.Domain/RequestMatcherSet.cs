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
    
    public async Task<RuleMatchResult> IsMatch(RequestData request)
    {
        int weightMethod = 0;
        int weightPath = 0;
        int weightQuery = 0;
        int weightBody = 0;
        int weightHeaders = 0;

        RequestMatchResult matchResult;
        if (_methodMatcher != null)
        {
            matchResult = await _methodMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatchedInstance;
            
            weightMethod = matched.Weight;
        }

        if (_pathMatcher != null)
        {
            matchResult = await _pathMatcher.IsMatch(request);
            if (matchResult is not Matched matchedPath)
                return RuleMatchResult.NotMatchedInstance;

            weightPath = matchedPath.Weight;
        }

        if (_queryStringMatcher != null)
        {
            matchResult = await _queryStringMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatchedInstance;
            weightQuery = matched.Weight;
        }

        if (_headersMatcher != null)
        {
            matchResult = await _headersMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatchedInstance;
            weightHeaders = matched.Weight;
        }

        if (_bodyMatcher != null)
        {
            matchResult = await _bodyMatcher.IsMatch(request);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatchedInstance;
            weightBody = matched.Weight;
        }

        return new RuleMatchResult.Matched(new RuleMatchWeight(
            weightMethod,
            weightPath,
            weightQuery,
            weightBody,
            weightHeaders
        ));
    }
}
