using ArgValidation;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Path;


public class PathRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyList<TextPatternPart> _segmentsPatterns;

    public PathRequestMatcher(IReadOnlyList<TextPatternPart> expectedSegments)
    {
        Arg.NotEmpty(expectedSegments, nameof(expectedSegments));
        _segmentsPatterns = expectedSegments;
    }

    public Task<bool> IsMatch(RequestData request)
    {
        var currentSegments = request.Path.Value.Split('/');

        if (_segmentsPatterns.Count != currentSegments.Length)
            return Task.FromResult(false);

        for (var i = 0; i < _segmentsPatterns.Count; i++)
        {
            var pattern = _segmentsPatterns[i];
            var current = currentSegments[i];
            var isMatch = pattern.IsMatch(current);

            if (!isMatch)
                return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
