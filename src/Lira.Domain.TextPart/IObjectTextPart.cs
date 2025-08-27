namespace Lira.Domain.TextPart;

public interface IObjectTextPart
{
    IEnumerable<dynamic?> Get(RuleExecutingContext context);
    ReturnType? ReturnType { get; }
}

public record StaticPart(string Value) : IObjectTextPart
{
    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return Value;
    }
    public ReturnType ReturnType => ReturnType.String;
}