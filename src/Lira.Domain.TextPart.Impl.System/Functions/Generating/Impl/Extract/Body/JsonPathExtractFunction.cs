using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract.Body;

class JsonPathExtractFunction : WithArgumentFunction<string>, IBodyExtractFunction, IObjectTextPart
{
    public override string Name => "req.body.jpath";
    public Type Type => DotNetType.String;
    public override bool ArgumentIsRequired => true;

    private string _jpath = "";

    public string? Extract(string? body) => BodyUtils.GetByJPath(body, _jpath);

    public dynamic? Get(RuleExecutingContext context)
    {
        return Extract(context.RequestContext.RequestData.ReadBody());
    }

    public override void SetArgument(string arguments) => _jpath = arguments;
}
