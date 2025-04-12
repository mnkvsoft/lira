using System.Collections.Immutable;

namespace Lira.Domain.TextPart.Impl.CSharp;

public record FunctionFactoryRuleContext(FunctionFactoryUsingContext UsingContext, IDeclaredPartsProvider DeclaredPartsProvider);

public class FunctionFactoryUsingContext
{
    public static readonly FunctionFactoryUsingContext Empty = new(ImmutableList<string>.Empty);
    public IReadOnlyCollection<string> FileUsings { get; }
    internal FunctionFactoryUsingContext(IReadOnlyCollection<string> fileUsings)
    {
        FileUsings = fileUsings;
    }
}

public interface IDeclaredPartsProvider
{
    IObjectTextPart Get(string name);

    ReturnType? GetPartType(string name);

    void SetVariable(string name, RuleExecutingContext context, dynamic value);
}