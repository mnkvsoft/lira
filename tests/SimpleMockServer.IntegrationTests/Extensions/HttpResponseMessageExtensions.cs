namespace SimpleMockServer.IntegrationTests.Extensions;

static class HttpResponseMessageExtensions
{
    public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetAllHeaders(this HttpResponseMessage response)
    {
        var headers = new Dictionary<string, IReadOnlyCollection<string>>();

        foreach (var header in response.Headers)
        {
            headers.Add(header.Key, new List<string>(header.Value));
        }

        if (response.Content?.Headers != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers.Add(header.Key, new List<string>(header.Value));
            }
        }

        return headers;
    }

}