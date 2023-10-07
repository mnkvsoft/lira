using ArgValidation;

namespace Lira.Domain.Matching.Request.Matchers;

public class HeadersRequestMatcher : IRequestMatcher
{
    IReadOnlyDictionary<string, TextPatternPart> _headers;

    public HeadersRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> headers)
    {
        Arg.NotEmpty(headers, nameof(headers));
        _headers = headers;
    }

    public Task<RequestMatchResult> IsMatch(RequestData request)
    {
        int weight = 0;
        foreach (var header in _headers)
        {
            var pattern = header.Value;
            var values = request.Headers[header.Key];

            var isMatch = false;
            foreach (var value in values)
            {
                if (pattern.IsMatch(value))
                {
                    isMatch = true;
                    break;
                }
            }

            if (!isMatch)
                return Task.FromResult(RequestMatchResult.NotMatched);
            
            weight += TextPatternPartWeightCalculator.Calculate(pattern);
            
        }
        return Task.FromResult(RequestMatchResult.Matched(weight));
    }
}
