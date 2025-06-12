namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;

internal class Path : WithArgumentFunction<int>, IObjectTextPart
{
    public override string Name => "req.path";
    public ReturnType ReturnType => ReturnType.String;
    public override bool ArgumentIsRequired => true;
    private int _index;

    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return context.RequestContext.RequestData.GetPath(_index);
    }


    public override void SetArgument(int arguments)
    {
        _index = arguments;
    }
}
