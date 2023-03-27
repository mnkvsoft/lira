using ArgValidation;
using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;

public class BodyRequestMatcher : IRequestMatcher
{
    private readonly IReadOnlyCollection<KeyValuePair<IExtractFunction, ValuePattern>> _extractToMatchFunctionMap;

    public BodyRequestMatcher(IReadOnlyCollection<KeyValuePair<IExtractFunction, ValuePattern>> extractToMatchFunctionMap)
    {
        Arg.NotEmpty(extractToMatchFunctionMap, nameof(extractToMatchFunctionMap)); 
        _extractToMatchFunctionMap = extractToMatchFunctionMap;
    }

    public async Task<bool> IsMatch(HttpRequest request)
    {
        request.Body.Position = 0;
        var stream = new StreamReader(request.Body);
        string body = await stream.ReadToEndAsync();

        foreach(var pair in _extractToMatchFunctionMap)
        {
            var extractor = pair.Key;
            var matcher = pair.Value;

            string? value = extractor.Extract(body);
            if (!matcher.IsMatch(value))
                return false;
        }

        return true;
    }

}
