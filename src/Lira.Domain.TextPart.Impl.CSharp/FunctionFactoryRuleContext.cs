using System.Collections.Immutable;
using Lira.Domain.TextPart.Impl.Custom;

namespace Lira.Domain.TextPart.Impl.CSharp;

public record FunctionFactoryRuleContext(FunctionFactoryUsingContext UsingContext, IDeclaredItemsProvider DeclaredItemsProvider);

public class FunctionFactoryUsingContext
{
    public static readonly FunctionFactoryUsingContext Empty = new(ImmutableList<string>.Empty);
    public IReadOnlyCollection<string> FileUsings { get; }
    internal FunctionFactoryUsingContext(IReadOnlyCollection<string> fileUsings)
    {
        FileUsings = fileUsings;
    }
}