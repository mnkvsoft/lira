using ArgValidation;

namespace Lira.Domain.Matching.Request.Matchers;

public class PathRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyList<TextPatternPart> _segmentsPatterns;

    public PathRequestMatcher(IReadOnlyList<TextPatternPart> expectedSegments)
    {
        Arg.NotEmpty(expectedSegments, nameof(expectedSegments));
        _segmentsPatterns = expectedSegments;
    }

    async Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext context)
    {
        var request = context.RequestData;
        var currentSegments = request.Path.Value.Split('/');

        if (_segmentsPatterns.Count != currentSegments.Length)
            return RequestMatchResult.NotMatched;

        int weight = 0;
        for (var i = 0; i < _segmentsPatterns.Count; i++)
        {
            var pattern = _segmentsPatterns[i];
            var current = currentSegments[i];

            if (await pattern.Match(context, current) == false)
                return RequestMatchResult.NotMatched;

            weight += TextPatternPartWeightCalculator.Calculate(pattern);
        }

        return RequestMatchResult.Matched(name: "path", weight);
    }
}
