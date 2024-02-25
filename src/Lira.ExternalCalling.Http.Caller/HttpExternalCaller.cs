using System.Text;
using Lira.Domain;
using Lira.Domain.Actions;
using Lira.Domain.Generating;

namespace Lira.ExternalCalling.Http.Caller;

public class HttpAction : IAction
{
    public const string HttpClientName = nameof(HttpAction);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpMethod _httpMethod;
    private readonly TextParts _requestUriParts;
    private readonly TextParts? _bodyParts;
    private readonly IReadOnlyCollection<GeneratingHeader>? _headers;

    public HttpAction(
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

    public async Task Execute(RuleExecutingContext context)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        var req = CreateRequest(context);

        await client.SendAsync(req);
    }

    private HttpRequestMessage CreateRequest(RuleExecutingContext context)
    {
        var req = new HttpRequestMessage();
        req.Method = _httpMethod;

        var url = _requestUriParts.Generate(context);

        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("Url can't be empty");

        req.RequestUri = new Uri(url);

        string? contentType = null;
        if (_headers != null)
        {
            foreach (var header in _headers)
            {
                string value = header.TextParts.Generate(context);
                
                if (header.Name == Header.ContentType)
                {
                    contentType = value;
                    continue;
                }
                
                req.Headers.Add(header.Name, value);
            }
        }

        if (string.IsNullOrWhiteSpace(contentType))
            throw new Exception("Header Content-Type is required");
        
        req.Content = new StringContent(_bodyParts?.Generate(context) ?? "", Encoding.UTF8, contentType);
        return req;
    }
}
