using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract.Body;

class JsonPathExtractFunction : WithArgumentFunction<string>, IBodyExtractFunction, IObjectTextPart
{
    public override string Name => "req.body.jpath";
    public ReturnType ReturnType => ReturnType.String;
    public override bool ArgumentIsRequired => true;

    private string _jpath = "";

    public string? Extract(string? body) => BodyUtils.GetByJPath(body, _jpath);

    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return Extract(context.RequestContext.RequestData.ReadBody());
    }

    public override void SetArgument(string arguments) => _jpath = arguments;
}
