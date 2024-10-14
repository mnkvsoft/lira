using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Lira.Domain;

public record RequestContext(RequestData RequestData)
{
    public readonly Guid RequestId = Guid.NewGuid();
}

public record RequestData
{
    public string Method { get; }
    public PathString Path { get; }

    public string? GetPath(int index)
    {
        int realIndex = index + 1;

        if (Path.Value.Length < realIndex)
            return null;

        return Path.Value.Split('/')[realIndex];
    }
    
    public string? GetHeader(string name)
    {
        if (Headers.TryGetValue(name, out StringValues values))
            return values.First();

        return null;
    }
    
    public string? GetQueryParam(string name)
    {
        if (Query.TryGetValue(name, out StringValues values))
            return values.First();

        return null;
    }
    
    public QueryString QueryString { get; }
    public IHeaderDictionary Headers { get; }
    public IQueryCollection Query { get; }

    private Stream? _originBodyStream;

    private MemoryStream? _savedBodyStream = null;

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
