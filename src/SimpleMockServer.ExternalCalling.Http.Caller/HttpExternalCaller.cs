using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Generating;

namespace SimpleMockServer.ExternalCalling.Http.Caller;

public class HttpExternalCaller : IExternalCaller
{
    public const string HttpClientName = nameof(HttpExternalCaller);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpMethod _httpMethod;
    private readonly TextParts _requestUriParts;
    private readonly TextParts? _bodyParts;
    private readonly IReadOnlyCollection<GeneratingHeader>? _headers;

    public HttpExternalCaller(
        IHttpClientFactory httpClientFactory,
        HttpMethod httpMethod,
        TextParts requestUriParts,
        TextParts? bodyParts,
        IReadOnlyCollection<GeneratingHeader>? headers)
    {
        _httpClientFactory = httpClientFactory;
        _httpMethod = httpMethod;
        _requestUriParts = requestUriParts;
        _bodyParts = bodyParts;
        _headers = headers;
    }

    public async Task Call(RequestData request)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        var req = CreateRequest(request);

        await client.SendAsync(req);
    }

    private HttpRequestMessage CreateRequest(RequestData request)
    {
        var req = new HttpRequestMessage();
        req.Method = _httpMethod;

        var url = _requestUriParts.Generate(request);

        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("Url can't be empty");

        req.RequestUri = new Uri(url);

        if (_headers != null)
        {
            foreach (var header in _headers)
            {
                req.Headers.Add(header.Name, header.TextParts.Generate(request));
            }
        }

        req.Content = new StringContent(_bodyParts?.Generate(request) ?? "");
        return req;
    }
}
