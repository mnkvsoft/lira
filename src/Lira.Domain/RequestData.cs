using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Lira.Domain;

public record RequestData
{
    public string Method { get; }
    public PathString Path { get; }

    private IReadOnlyCollection<PathNameMap>? _pathNameMaps;

    public IReadOnlyCollection<PathNameMap>? PathNameMaps
    {
        get => _pathNameMaps;
        set
        {
            if (_pathNameMaps != null)
                throw new Exception(nameof(PathNameMaps) + " already set");
            _pathNameMaps = value;
        }
    }
    
    public string GetPathSegmentValue(string name)
    {
        var map = PathNameMaps?.FirstOrDefault(x => x.Name == name);
        if (map == null)
            throw new Exception($"Path segment with name '{name}' not defined");

        return Path.Value.Split('/')[map.Index];
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
