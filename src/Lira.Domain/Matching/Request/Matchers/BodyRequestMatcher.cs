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

    async Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext ctx)
    {
        var request = ctx.RequestData;
        request.Body.Position = 0;
        var stream = new StreamReader(request.Body);
        var body = await stream.ReadToEndAsync();

        int weight = 0;
        foreach (var pair in _extractToMatchFunctionMap)
        {
            var extractor = pair.Key;
            var matcher = pair.Value;

            var value = extractor.Extract(body);

            if (await matcher.Match(ctx, value) == false)
                return RequestMatchResult.NotMatched;

            weight += TextPatternPartWeightCalculator.Calculate(matcher);
        }

        return RequestMatchResult.Matched(name: "body", weight);
    }

}
