using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract.Body;

class AllExtractFunction : FunctionBase, IBodyExtractFunction, IObjectTextPart
{
    public override string Name => "req.body.all";
    public Type Type => DotNetType.String;

    public string? Extract(string? value)
    {
        return value;
    }

    public dynamic Get(RuleExecutingContext context)
    {
        return context.RequestContext.RequestData.ReadBody();
    }
}
