using Microsoft.AspNetCore.Http;

namespace Lira.Domain.Generating.Writers;

interface IResponseWriter
{
    Task WriteBody(string text);
    void AddHeader(string name, string value);
    int StatusCode { set; }
}

class OriginalResponseWriter : IResponseWriter
{
    private readonly HttpResponse _response;

    public OriginalResponseWriter(HttpResponse response) => _response = response;

    public Task WriteBody(string text) => _response.WriteAsync(text);

    public void AddHeader(string name, string value)
    {
        if (name == "Content-Type")
            _response.ContentType = value;
        else
            _response.Headers.Add(name, value);
    }

    public int StatusCode
    {
        set => _response.StatusCode = value;
    }
}