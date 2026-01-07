using System.Text;

namespace Lira.Domain.Handling;

internal class SavingResponseWriter(IResponseWriter original) : IResponseWriter
{
    private Dictionary<string, string?>? _headers;
    private List<string>? _bodyParts;

    private int? _code;
    private bool? _isAborted;

    public void WriteCode(int code)
    {
        _code = code;
        original.WriteCode(code);
    }

    public void WriteHeader(Header header)
    {
        var name = header.Name;
        _headers ??= new Dictionary<string, string?>();
        _headers.TryGetValue(name, out var val);
        _headers[name] = val == null ? header.Value : val + "," + header.Value;

        original.WriteHeader(header);
    }

    public async Task WriteBody(string part, Encoding encoding)
    {
        _bodyParts ??= new List<string>();
        _bodyParts.Add(part);

        await original.WriteBody(part, encoding);
    }

    public void Abort()
    {
        _isAborted = true;
        original.Abort();
    }

    public Response GetRequestHandleResult()
    {
        return _isAborted == true
            ? new Response.Fault()
            : new Response.Normal(
                _code ?? throw new Exception("Code is null"),
                _headers,
                _bodyParts == null
                    ? null
                    : string.Concat(_bodyParts));
    }
}