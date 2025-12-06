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

    async Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext context)
    {
        var request = context.RequestData;
        int weight = 0;

        foreach (var header in _headers)
        {
            var pattern = header.Value;
            var values = request.Headers[header.Key];

            var isMatch = false;
            foreach (var value in values)
            {
                if (await pattern.Match(context, value))
                {
                    isMatch = true;
                    break;
                }
            }

            if (!isMatch)
                return RequestMatchResult.NotMatched;

            weight += TextPatternPartWeightCalculator.Calculate(pattern);

        }
        return RequestMatchResult.Matched(name: "headers", weight);
    }
}
