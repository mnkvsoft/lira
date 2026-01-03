using System.Text;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

class ResponseData(HttpResponse response)
{
    public bool NeedSaveData { get; set; }

    private int? _statusCode;
    private Dictionary<string, string?>? _headers;
    private StringBuilder? _body;

    public int StatusCode
    {
        get
        {
            if (_statusCode == null)
                throw new Exception("Status code was not set");

            return _statusCode.Value;
        }
        set
        {
            if (NeedSaveData)
                _statusCode = value;

            _statusCode = value;
            response.StatusCode = value;
        }
    }

    public IReadOnlyDictionary<string, string?>? Headers => _headers;
    public string? Body => _body?.ToString();

    public void AddHeader(Header header)
    {
        var name = header.Name;
        if (NeedSaveData)
        {
            _headers ??= new Dictionary<string, string?>();

            _headers.TryGetValue(name, out var value);
            _headers[name] = value == null ? header.Value : value + "," + header.Value;
        }

        if(response.Headers.ContainsKey(name))
            response.Headers.Append(name, header.Value);
        else
            response.Headers.Add(name, header.Value);
    }

    public async Task WriteBody(string part, Encoding encoding)
    {
        if (NeedSaveData)
        {
            _body ??= new StringBuilder();
            _body.Append(part);
        }

        await response.WriteAsync(part, encoding);
    }

    public void Abort()
    {
        response.HttpContext.Abort();
    }
}

internal readonly record struct Header(string Name, string? Value);