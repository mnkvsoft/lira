namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;

internal class Path : WithArgumentFunction<int>, IObjectTextPart
{
    public override string Name => "req.path";
    public override bool ArgumentIsRequired => true;
    private int _index = 0;

    public dynamic? Get(RuleExecutingContext context) => context.Request.GetPath(_index);


    public override void SetArgument(int argument)
    {
        _index = argument;
    }
}
