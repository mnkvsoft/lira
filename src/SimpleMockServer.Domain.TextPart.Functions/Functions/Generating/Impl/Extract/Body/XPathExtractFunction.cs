using SimpleMockServer.Domain.Extensions;
using SimpleMockServer.Domain.Matching.Request.Matchers;
using SimpleMockServer.Domain.TextPart.Functions.Utils;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body;

public class XPathExtractFunction : IBodyExtractFunction, IObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "req.body.xpath";

    private string _xpath = "";

    public string? Extract(string? body) => BodyUtils.GetByXPath(body, _xpath);

    public object? Get(RequestData request) => Extract(request.ReadBody());

    public void SetArgument(string argument) => _xpath = argument;
}
