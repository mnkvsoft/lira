namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;
internal class Header : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.header";
    public Type Type => DotNetType.String;
    public override bool ArgumentIsRequired => true;
    private string _headerName = "";

    public dynamic? Get(RuleExecutingContext context)
    {
        return context.RequestContext.RequestData.GetHeader(_headerName);
    }

    public override void SetArgument(string arguments)
    {
        _headerName = arguments;
    }
}
