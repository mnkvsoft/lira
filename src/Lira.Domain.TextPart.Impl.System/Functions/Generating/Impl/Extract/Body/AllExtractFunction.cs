using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract.Body;

class AllExtractFunction : FunctionBase, IBodyExtractFunction, IObjectTextPart
{
    public override string Name => "req.body.all";

    public string? Extract(string? value)
    {
        return value;
    }

    public object Get(RequestData request)
    {
        return request.ReadBody();
    }
}
