using ArgValidation;
using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Path;


public class PathRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyList<ValuePattern> _segmentsPatterns;

    public PathRequestMatcher(IReadOnlyList<ValuePattern> expectedSegments)
    {
        Arg.NotEmpty(expectedSegments, nameof(expectedSegments));
        _segmentsPatterns = expectedSegments;
    }

    public Task<bool> IsMatch(HttpRequest request)
    {
        string[] currentSegments = request.Path.Value.Split('/');

        if (_segmentsPatterns.Count != currentSegments.Length)
            return Task.FromResult(false);

        for (int i = 0; i < _segmentsPatterns.Count; i++)
        {
            ValuePattern pattern = _segmentsPatterns[i];
            string current = currentSegments[i];
            bool isMatch = pattern.IsMatch(current);

            if (!isMatch)
                return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}