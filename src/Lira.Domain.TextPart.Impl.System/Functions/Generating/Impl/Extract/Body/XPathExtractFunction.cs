using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract.Body;

class XPathExtractFunction : WithArgumentFunction<string>, IBodyExtractFunction, IObjectTextPart
{
    public override string Name => "req.body.xpath";
    public ReturnType ReturnType => ReturnType.String;
    public override bool ArgumentIsRequired => true;

    private string _xpath = "";

    public string? Extract(string? body) => BodyUtils.GetByXPath(body, _xpath);

    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return Extract(context.RequestContext.RequestData.ReadBody());
    }

    public override void SetArgument(string arguments) => _xpath = arguments;
}
