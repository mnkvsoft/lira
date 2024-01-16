using ArgValidation;

namespace Lira.Domain.Matching.Request.Matchers;

public interface IBodyExtractFunction
{
    string? Extract(string? body);
}

public class BodyRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyCollection<KeyValuePair<IBodyExtractFunction, TextPatternPart>> _extractToMatchFunctionMap;

    public BodyRequestMatcher(IReadOnlyCollection<KeyValuePair<IBodyExtractFunction, TextPatternPart>> extractToMatchFunctionMap)
    {
        Arg.NotEmpty(extractToMatchFunctionMap, nameof(extractToMatchFunctionMap));
        _extractToMatchFunctionMap = extractToMatchFunctionMap;
    }

    public async Task<RequestMatchResult> IsMatch(RequestData request)
    {
        request.Body.Position = 0;
        var stream = new StreamReader(request.Body);
        var body = await stream.ReadToEndAsync();

        int weight = 0;
        foreach (var pair in _extractToMatchFunctionMap)
        {
            var extractor = pair.Key;
            var matcher = pair.Value;

            var value = extractor.Extract(body);

            if(value == null)
                return RequestMatchResult.NotMatched;

            if (!matcher.IsMatch(value))
                return RequestMatchResult.NotMatched;
            
            weight += TextPatternPartWeightCalculator.Calculate(matcher);
        }

        return RequestMatchResult.Matched(weight);
    }

}
