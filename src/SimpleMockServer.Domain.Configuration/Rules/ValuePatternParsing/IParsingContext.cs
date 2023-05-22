using System.Runtime.Loader;
using SimpleMockServer.Domain.Configuration.Templating;
using SimpleMockServer.Domain.TextPart.CSharp;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.RuntimeCompilation;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;



public interface IParsingContext
{
}
    
record ParsingContext(
    CSharpCodeRegistry CSharpCodeRegistry,
    IReadOnlyCollection<Variable> Variables, 
    IReadOnlyCollection<Template> Templates, 
    string RootPath, 
    string CurrentPath) : IParsingContext;

internal static class ParsingContextExtensions
{
    public static ParsingContext ToImpl(this IParsingContext context) => (ParsingContext)context;
}
