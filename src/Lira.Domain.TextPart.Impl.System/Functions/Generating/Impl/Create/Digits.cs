namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Digits : WithArgumentFunction<int>, IObjectTextPart
{
    public override string Name => "digits";
    public override bool ArgumentIsRequired => false;

    private int _length = 10;

    public dynamic Get(RuleExecutingContext context) => GetInternal();

    private IEnumerable<dynamic> GetInternal()
    {
        for (int i = 0; i < _length; i++)
        {
            yield return StrDigits[Random.Shared.Next(0, StrDigits.Length)];
        }
    }

    public Type Type => DotNetType.EnumerableDynamic;

    public override void SetArgument(int arguments)
    {
        _length = arguments;
    }

    private static readonly string[] StrDigits =
        ["1","2","3","4","5","6","7","8","9","0"];
}
