using SimpleMockServer.Domain.Extensions;
using SimpleMockServer.Domain.Matching.Request.Matchers;
using SimpleMockServer.Domain.TextPart.Functions.Utils;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body;

public class JsonPathExtractFunction : IBodyExtractFunction, IObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "read.req.body.jpath";

    private string _jpath = "";

    public string? Extract(string? body) => BodyUtils.GetByJPath(body, _jpath);

    public object? Get(RequestData request) => Extract(request.ReadBody());

    public void SetArgument(string argument) => _jpath = argument;
}
