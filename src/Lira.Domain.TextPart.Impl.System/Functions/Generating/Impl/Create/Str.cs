using System.Text;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Str : WithArgumentFunction<int>, IObjectTextPart
{
    public override string Name => "str";
    public override bool ArgumentIsRequired => false;

    private int _length = 20;

    public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(GetRandomString(_length));
    public ReturnType ReturnType => ReturnType.String;

    public override void SetArgument(int argument)
    {
        _length = argument;
    }

    private static string GetRandomString(int length)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            char randomChar = RandomSymbols[Random.Shared.Next(0, RandomSymbols.Length)];
            sb.Append(randomChar);
        }

        return sb.ToString();
    }

    private static readonly string RandomSymbols =
        "1234567890" +
        "qwertyuiop" +
        "asdfghjkl" +
        "zxcvbnm";
}
