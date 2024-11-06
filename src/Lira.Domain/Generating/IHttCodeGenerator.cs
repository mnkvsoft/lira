namespace Lira.Domain.Generating;

public interface IHttCodeGenerator
{
    int Generate(RuleExecutingContext context);
}

public record StaticHttCodeGenerator(int Code) : IHttCodeGenerator
{
    public static readonly StaticHttCodeGenerator Code200 = new(200);

    public int Generate(RuleExecutingContext context) => Code;
}

public record DynamicHttCodeGenerator(TextParts Parts) : IHttCodeGenerator
{
    public static readonly StaticHttCodeGenerator Code200 = new(200);

    public int Generate(RuleExecutingContext context)
    {
        string strCode = Parts.Generate(context);
        if(!int.TryParse(strCode, out var code))
            throw new Exception("Invalid http code: '" + strCode + "'");

        if(code is < 100 or > 599)
            throw new Exception("Invalid http code: " + strCode + "");

        return code;
    }
}