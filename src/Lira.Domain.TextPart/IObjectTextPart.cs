namespace Lira.Domain.TextPart;

public interface IObjectTextPart
{
    IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context);
    ReturnType? ReturnType { get; }
}

public record StaticPart(string Value) : IObjectTextPart
{
    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return await ValueTask.FromResult(Value);
    }
    public ReturnType ReturnType => ReturnType.String;
}