namespace Lira.Domain.Matching.Request.Matchers;

public class HeadersRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> headers) : IRequestMatcher
{
    Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext context)
    {
        var request = context.RequestContext.RequestData;
        int weight = 0;

        foreach (var header in headers)
        {
            var pattern = header.Value;
            var values = request.Headers[header.Key];

            var isMatch = false;
            foreach (var value in values)
            {
                if (pattern.Match(context, value) is TextPatternPart.MatchResult.Matched)
                {
                    isMatch = true;
                    break;
                }
            }

            if (!isMatch)
                return Task.FromResult(RequestMatchResult.NotMatched);

            weight += TextPatternPartWeightCalculator.Calculate(pattern);

        }
        return Task.FromResult(RequestMatchResult.Matched(name: "headers", weight));
    }
}
