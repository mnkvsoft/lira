namespace Lira.Domain.Handling.Generating;

public interface IHttCodeGenerator
{
    int Generate(RuleExecutingContext context);
}

public record StaticHttCodeGenerator(int Code) : IHttCodeGenerator
{
    public static readonly StaticHttCodeGenerator Code200 = new(200);

    public int Generate(RuleExecutingContext context) => Code;
}

public record DynamicHttCodeGenerator(TextPartsProvider PartsProvider) : IHttCodeGenerator
{
    public int Generate(RuleExecutingContext context)
    {
        string strCode = PartsProvider.GetSingleString(context);
        return strCode.ToHttpCode();
    }
}

public static class HttCodeStringExtensions
{
    public static int ToHttpCode(this string? str)
    {
        if(!int.TryParse(str, out var code))
            throw new Exception("Invalid http code: '" + str + "'");

        if(code is < 100 or > 599)
            throw new Exception("Invalid http code: " + str + "");

        return code;
    }
}