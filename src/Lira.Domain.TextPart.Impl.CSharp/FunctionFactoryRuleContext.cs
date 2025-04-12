using System.Collections.Immutable;

namespace Lira.Domain.TextPart.Impl.CSharp;

public record FunctionFactoryRuleContext(FunctionFactoryUsingContext UsingContext, IDeclaredPartsProvider DeclaredPartsProvider);

public record FunctionFactoryUsingContext(IReadOnlyCollection<string> FileUsings)
{
    public static readonly FunctionFactoryUsingContext Empty = new(ImmutableList<string>.Empty);
}

public interface IDeclaredPartsProvider
{
    IObjectTextPart Get(string name);

    ReturnType? GetPartType(string name);

    void SetVariable(string name, RuleExecutingContext context, dynamic value);
}