using Lira.Domain.Configuration.Templating;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IParsingContext
{
}

record ParsingContext(
    IReadonlyDeclaredItems DeclaredItems,
    IReadOnlyCollection<Template> Templates,
    TextPart.Impl.Custom.CustomSetModel.CustomSets CustomSets,
    string RootPath,
    string CurrentPath) : IParsingContext;

internal static class ParsingContextExtensions
{
    public static ParsingContext ToImpl(this IParsingContext context) => (ParsingContext)context;
}
