using ArgValidation;

namespace SimpleMockServer.Domain.Matching.Request.Matchers.Headers;

internal class HeadersRequestMatcher : IRequestMatcher
{
    IReadOnlyDictionary<string, TextPatternPart> _headers;

    public HeadersRequestMatcher(IReadOnlyDictionary<string, TextPatternPart> headers)
    {
        Arg.NotEmpty(headers, nameof(headers));
        _headers = headers;
    }

    public Task<bool> IsMatch(RequestData request)
    {
        foreach (var header in _headers)
        {
            var pattern = header.Value;
            var values = request.Headers[header.Key];

            var isMatch = false;
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