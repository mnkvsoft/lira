using ArgValidation;
using Microsoft.AspNetCore.Http;
using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.Domain.Matching.Matchers.Path;

public class PathRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyList<IValuePattern> _segmentsPatterns;

    public PathRequestMatcher(IReadOnlyList<IValuePattern> expectedSegments)
    {
        Arg.NotEmpty(expectedSegments, nameof(expectedSegments));
        _segmentsPatterns = expectedSegments;
    }

    public bool IsMatch(HttpRequest request)
    {
        string[] currentSegments = request.Path.Value.Split('/');

        if (_segmentsPatterns.Count != currentSegments.Length)
            return false;

        for (int i = 0; i < _segmentsPatterns.Count; i++)
        {
            IValuePattern pattern = _segmentsPatterns[i];
            string current = currentSegments[i];
            bool isMatch = pattern.IsMatch(current);

            if (!isMatch)
                return false;
        }

        return true;
    }
}