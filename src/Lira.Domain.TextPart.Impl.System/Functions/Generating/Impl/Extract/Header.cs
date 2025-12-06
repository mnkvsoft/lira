namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;
internal class Header : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.header";
    public ReturnType ReturnType => ReturnType.String;
    public override bool ArgumentIsRequired => true;
    private string _headerName = "";

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return context.RequestData.GetHeader(_headerName);
    }

    public override void SetArgument(string arguments)
    {
        _headerName = arguments;
    }
}
