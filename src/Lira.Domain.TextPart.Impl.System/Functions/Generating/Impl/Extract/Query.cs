namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;

internal class Query : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.query";
    public ReturnType ReturnType => ReturnType.String;
    public override bool ArgumentIsRequired => true;

    private string _queryParamName = "";

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return context.RequestContext.RequestData.GetQueryParam(_queryParamName);
    }

    public override void SetArgument(string arguments)
    {
        _queryParamName = arguments;
    }
}