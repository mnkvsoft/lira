namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;
internal class Header : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.header";
    public ReturnType ReturnType => ReturnType.String;
    public override bool ArgumentIsRequired => true;
    private string _headerName = "";

    public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(context.RequestContext.RequestData.GetHeader(_headerName));


    public override void SetArgument(string argument)
    {
        _headerName = argument;
    }
}
