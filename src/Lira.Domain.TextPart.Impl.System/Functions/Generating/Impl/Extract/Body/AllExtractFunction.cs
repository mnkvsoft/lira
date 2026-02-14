using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract.Body;

class AllExtractFunction : FunctionBase, IBodyExtractFunction, IObjectTextPart
{
    public override string Name => "req.body.all";
    public ReturnType ReturnType => ReturnType.String;

    public string? Extract(string? value)
    {
        return value;
    }

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return context.RequestData.ReadBody();
    }
}