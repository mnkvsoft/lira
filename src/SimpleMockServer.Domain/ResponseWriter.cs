using ArgValidation;
using SimpleMockServer.Domain.Generating.Writers;

namespace SimpleMockServer.Domain;

public class ResponseWriter
{
    private readonly int _code;
    private readonly BodyWriter? _bodyWriter;
    private readonly HeadersWriter? _headersWriter;

    public ResponseWriter(int code, BodyWriter? bodyWriter, HeadersWriter? headersWriter)
    {
        Arg.Validate(code, nameof(code))
            .InRange(100, 599);

        _code = code;
        _bodyWriter = bodyWriter;
        _headersWriter = headersWriter;
    }

    public async Task Write(HttpContextData httpContextData)
    {
        var response = httpContextData.Response;

        _headersWriter?.Write(httpContextData);
        response.StatusCode = _code;

        if (_bodyWriter != null)
            await _bodyWriter.Write(httpContextData);
    }
}
