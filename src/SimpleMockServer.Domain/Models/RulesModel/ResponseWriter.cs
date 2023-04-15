using ArgValidation;
using SimpleMockServer.Domain.Models.RulesModel.Generating.Writers;

namespace SimpleMockServer.Domain.Models.RulesModel;

public class ResponseWriter
{
    private readonly int _code;
    private readonly BodyWriter? _bodyWriter;
    private readonly HeadersWriter? _headersWriter;
    private readonly TimeSpan? _delay;

    public ResponseWriter(int code, BodyWriter? bodyWriter, HeadersWriter? headersWriter, TimeSpan? delay)
    {
        Arg.Validate(code, nameof(code))
            .InRange(100, 599);

        _code = code;
        _bodyWriter = bodyWriter;
        _headersWriter = headersWriter;
        _delay = delay;
    }

    public async Task Write(HttpContextData httpContextData)
    {
        if (_delay != null)
            await Task.Delay(_delay.Value);

        var response = httpContextData.Response;

        _headersWriter?.Write(httpContextData);
        response.StatusCode = _code;

        if(_bodyWriter != null)
            await _bodyWriter.Write(httpContextData);
    }
}
