using ArgValidation;

namespace Lira.Domain.Matching.Request.Matchers;

public class HeadersRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyDictionary<string, TextPatternPart> _headers;

    public HeadersRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> headers)
    {
        Arg.NotEmpty(headers, nameof(headers));
        _headers = headers;
    }

    Task<RequestMatchResult> IRequestMatcher.IsMatch(RequestContext context)
    {
        var request = context.RequestData; ;
        var matchedValuesSet = new Dictionary<string, string?>();
        int weight = 0;

        foreach (var header in _headers)
        {
            var pattern = header.Value;
            var values = request.Headers[header.Key];

            var isMatch = false;
            foreach (var value in values)
            {
                if (pattern.Match(value) is TextPatternPart.MatchResult.Matched matched)
                {
                    matchedValuesSet.AddIfValueIdNotNull(matched);
                    isMatch = true;
                    break;
                }
            }

            if (!isMatch)
                return Task.FromResult(RequestMatchResult.NotMatched);
            
            weight += TextPatternPartWeightCalculator.Calculate(pattern);
            
        }
        return Task.FromResult(RequestMatchResult.Matched(name: "headers", weight, matchedValuesSet));
    }
}
