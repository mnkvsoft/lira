namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;
internal class Header : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.header";
    public override bool ArgumentIsRequired => true;
    private string _headerName = "";

    public dynamic? Get(RuleExecutingContext context) => context.RequestContext.RequestData.GetHeader(_headerName);


    public override void SetArgument(string argument)
    {
        _headerName = argument;
    }
}
