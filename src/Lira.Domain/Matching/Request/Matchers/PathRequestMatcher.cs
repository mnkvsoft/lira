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

    Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext context)
    {
        var request = context.RequestContext.RequestData;
        var currentSegments = request.Path.Value.Split('/');

        if (_segmentsPatterns.Count != currentSegments.Length)
            return Task.FromResult(RequestMatchResult.NotMatched);

        int weight = 0;
        for (var i = 0; i < _segmentsPatterns.Count; i++)
        {
            var pattern = _segmentsPatterns[i];
            var current = currentSegments[i];
            var matchResult = pattern.Match(context, current);

            if (matchResult is not TextPatternPart.MatchResult.Matched)
                return Task.FromResult(RequestMatchResult.NotMatched);

            weight += TextPatternPartWeightCalculator.Calculate(pattern);
        }

        return Task.FromResult(RequestMatchResult.Matched(name: "path", weight));
    }
}
