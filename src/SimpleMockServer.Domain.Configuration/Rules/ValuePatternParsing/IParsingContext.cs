using SimpleMockServer.Domain.Configuration.Templating;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

public interface IParsingContext
{
}
    
record ParsingContext(
    IReadonlyDeclaredItems DeclaredItems, 
    IReadOnlyCollection<Template> Templates, 
    string RootPath, 
    string CurrentPath) : IParsingContext;

internal static class ParsingContextExtensions
{
    public static ParsingContext ToImpl(this IParsingContext context) => (ParsingContext)context;
}
