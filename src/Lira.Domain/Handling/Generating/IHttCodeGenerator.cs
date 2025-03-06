namespace Lira.Domain.Handling.Generating;

public interface IHttCodeGenerator
{
    Task<int> Generate(RuleExecutingContext context);
}

public record StaticHttCodeGenerator(int Code) : IHttCodeGenerator
{
    public static readonly StaticHttCodeGenerator Code200 = new(200);

    public Task<int> Generate(RuleExecutingContext context) => Task.FromResult(Code);
}

public record DynamicHttCodeGenerator(TextParts Parts) : IHttCodeGenerator
{
    public async Task<int> Generate(RuleExecutingContext context)
    {
        string strCode = await Parts.Generate(context);
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