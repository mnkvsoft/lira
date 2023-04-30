using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel;

public record RequestData
{
    public string Method { get; }
    public PathString Path { get; }
    public QueryString QueryString { get; }
    public IHeaderDictionary Headers { get; }
    public IQueryCollection Query { get; }

    private Stream? _originBodyStream;

    private MemoryStream? _savedBodyStream = null;

    public IDictionary<string, object> Items { get; }

    public MemoryStream Body
    {
        get
        {
            if (_savedBodyStream == null)
            {
                SaveBody().Wait();
            }
            return _savedBodyStream!;
        }
    }

    public RequestData(string method, PathString path, QueryString queryString, IHeaderDictionary headers, IQueryCollection query, Stream body)
    {
        Method = method;
        Path = path;
        QueryString = queryString;
        Headers = headers;
        _originBodyStream = body;
        Query = query;
        Items = new Dictionary<string, object>();
    }

    public async Task SaveBody()
    {
        if (_savedBodyStream != null)
            return;

        var memoryStream = new MemoryStream();
        _originBodyStream!.Position = 0;
        await _originBodyStream!.CopyToAsync(memoryStream);
        _originBodyStream = null;
        _savedBodyStream = memoryStream;
    }
}
