namespace SimpleMockServer.Domain.Matching.Matchers.Headers;

internal class HeadersRequestMatcher : IRequestMatcher
{
    IReadOnlyDictionary<string, IValuePattern> _headers;

    public HeadersRequestMatcher(IReadOnlyDictionary<string, IValuePattern> headers)
    {
        _headers = headers;
    }

    public bool IsMatch(RequestData data)
    {
        foreach (var header in _headers)
        {
            var pattern = header.Value;
            var values = data.Headers[header.Key];

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
                return false;
        }
        return true;
    }
}