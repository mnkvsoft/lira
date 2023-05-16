using System.Text;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create.Random;

internal class RandomString : IGlobalObjectTextPart, IWithIntArgumentFunction, IWithOptionalArgument
{
    public static string Name => "random.string";
    private int _length = 20;
    
    public object Get(RequestData request) => Get();

    public object Get()
    {
        return GetRandomString(_length);
    }

    public void SetArgument(int argument)
    {
        _length = argument;
    }

    private static string GetRandomString(int length)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            char randomChar = RandomSymbols[Random.Next(0, RandomSymbols.Length - 1)];
            sb.Append(randomChar);
        }

        return sb.ToString();
    }

    private static readonly System.Random Random = new();
    
    private static readonly string RandomSymbols =
        "1234567890" +
        "qwertyuiop" +
        "asdfghjkl" +
        "zxcvbnm";
}
