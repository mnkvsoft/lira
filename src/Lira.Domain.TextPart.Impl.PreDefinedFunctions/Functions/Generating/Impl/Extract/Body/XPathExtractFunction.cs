using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract.Body;

public class XPathExtractFunction : IBodyExtractFunction, IObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "req.body.xpath";

    private string _xpath = "";

    public string? Extract(string? body) => BodyUtils.GetByXPath(body, _xpath);

    public object? Get(RequestData request) => Extract(request.ReadBody());

    public void SetArgument(string argument) => _xpath = argument;
}
