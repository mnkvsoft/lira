using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing.Extensions;

static class VariablesCollectionExtensions
{
    public static Variable GetOrThrow(this IReadOnlyCollection<Variable> variables, string name)
    {
        var result = variables.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Variable '{name}' not declared");
        return result;
    }
}
