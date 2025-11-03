namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;

internal class Query : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.query";
    public Type Type => DotNetType.String;
    public override bool ArgumentIsRequired => true;

    private string _queryParamName = "";

    public dynamic? Get(RuleExecutingContext context)
    {
        return context.RequestContext.RequestData.GetQueryParam(_queryParamName);
    }

    public override void SetArgument(string arguments)
    {
        _queryParamName = arguments;
    }
}