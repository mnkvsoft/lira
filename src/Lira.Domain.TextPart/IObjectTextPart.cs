namespace Lira.Domain.TextPart;

public interface IObjectTextPartCasted
{
    dynamic? Get(RuleExecutingContext context);
}

public interface IObjectTextPart
{
    dynamic? Get(RuleExecutingContext context);
    Type Type { get; }
}

public record StaticPart(string Value) : IObjectTextPart
{
    public dynamic Get(RuleExecutingContext context) => Value;
    public Type Type => typeof(string);
}

