using System.Text;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public class ResponseData(HttpResponse response)
{
    public bool NeedSaveData { get; set; }

    private int? _statusCode;
    private List<Header>? _headers;
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

    public IReadOnlyCollection<Header>? Headers => _headers;
    public string? Body => _body?.ToString();

    public void AddHeader(Header header)
    {
        if (NeedSaveData)
        {
            _headers ??= new List<Header>();
            _headers.Add(header);
        }
        response.Headers.Add(header.Name, header.Value);
    }

    public async Task WriteBody(string part)
    {
        if (NeedSaveData)
        {
            _body ??= new StringBuilder();
            _body.Append(part);
            await response.WriteAsync(part);
        }
        else
        {
            await response.WriteAsync(part);
        }
    }

    public void Abort()
    {
        response.HttpContext.Abort();
    }
}

public readonly record struct Header(string Name, string? Value);