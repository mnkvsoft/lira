using SimpleMockServer.Domain.Configuration.Templating;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

public interface IParsingContext
{
}
    
record ParsingContext(
    IReadOnlyCollection<Variable> Variables, 
    IReadOnlyCollection<Template> Templates, 
    string RootPath, 
    string CurrentPath) : IParsingContext;

internal static class ParsingContextExtensions
{
    public static ParsingContext ToImpl(this IParsingContext context) => (ParsingContext)context;
}
