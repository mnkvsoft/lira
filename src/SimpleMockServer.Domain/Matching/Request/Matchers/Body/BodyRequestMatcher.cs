using ArgValidation;

namespace SimpleMockServer.Domain.Matching.Request.Matchers.Body;

public class BodyRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyCollection<KeyValuePair<IBodyExtractFunction, TextPatternPart>> _extractToMatchFunctionMap;

    public BodyRequestMatcher(IReadOnlyCollection<KeyValuePair<IBodyExtractFunction, TextPatternPart>> extractToMatchFunctionMap)
    {
        Arg.NotEmpty(extractToMatchFunctionMap, nameof(extractToMatchFunctionMap));
        _extractToMatchFunctionMap = extractToMatchFunctionMap;
    }

    public async Task<bool> IsMatch(RequestData request)
    {
        request.Body.Position = 0;
        var stream = new StreamReader(request.Body);
        var body = await stream.ReadToEndAsync();

        foreach (var pair in _extractToMatchFunctionMap)
        {
            var extractor = pair.Key;
            var matcher = pair.Value;

            var value = extractor.Extract(body);
            if (!matcher.IsMatch(value))
                return false;
        }

        return true;
    }

}
