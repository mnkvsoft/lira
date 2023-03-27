using ArgValidation;
using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Headers;

internal class HeadersRequestMatcher : IRequestMatcher
{
    IReadOnlyDictionary<string, ValuePattern> _headers;

    public HeadersRequestMatcher(IReadOnlyDictionary<string, ValuePattern> headers)
    {
        Arg.NotEmpty(headers, nameof(headers));
        _headers = headers;
    }

    public Task<bool> IsMatch(HttpRequest request)
    {
        foreach (var header in _headers)
        {
            var pattern = header.Value;
            var values = request.Headers[header.Key];

            bool isMatch = false;
            foreach (var value in values)
            {
                if (pattern.IsMatch(value))
                {
                    isMatch = true;
                    break;
                }
            }

            if (!isMatch)
                return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
}