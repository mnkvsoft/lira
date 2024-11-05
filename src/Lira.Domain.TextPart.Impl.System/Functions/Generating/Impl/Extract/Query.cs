namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;

internal class Query : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.query";
    public override bool ArgumentIsRequired => true;

    private string _queryParamName = "";

    public object? Get(RuleExecutingContext context) => context.RequestContext.RequestData.GetQueryParam(_queryParamName);


    public override void SetArgument(string argument)
    {
        _queryParamName = argument;
    }
}

internal class Value : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "value";
    public override bool ArgumentIsRequired => true;

    private string _nameId = "";

    public object? Get(RuleExecutingContext context) => context.GetValue(_nameId);

    public override void SetArgument(string argument)
    {
        _nameId = argument;
    }
}
