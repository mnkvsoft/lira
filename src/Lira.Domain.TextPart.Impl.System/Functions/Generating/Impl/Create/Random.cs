namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;
internal class RandomCustom : WithArgumentFunction<string[]>, IObjectTextPart
{
    public override string Name => "random";
    private string[] _values = null!;

    public override bool ArgumentIsRequired => true;

    public object Get(RequestData request) => _values[Random.Shared.Next(0, _values.Length)];

    public override void SetArgument(string[] argument)
    {
        if (argument.Length == 0)
            throw new Exception("Not empty array required for function 'random'");
        _values = argument;
    }
}
