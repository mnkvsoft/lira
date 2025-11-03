namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;

internal class Path : WithArgumentFunction<int>, IObjectTextPart
{
    public override string Name => "req.path";
    public Type Type => DotNetType.String;
    public override bool ArgumentIsRequired => true;
    private int _index;

    public dynamic? Get(RuleExecutingContext context)
    {
        return context.RequestContext.RequestData.GetPath(_index);
    }


    public override void SetArgument(int arguments)
    {
        _index = arguments;
    }
}
