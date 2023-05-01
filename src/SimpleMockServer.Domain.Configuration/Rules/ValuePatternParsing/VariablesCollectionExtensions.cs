namespace SimpleMockServer.Domain.TextPart.Variables;

static class VariablesCollectionExtensions
{
    public static TVariable GetOrThrow<TVariable>(this IReadOnlyCollection<TVariable> variables, string name) where TVariable : Variable
    {
        var result = variables.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Variable '{name}' not declared");
        return result;
    }
}

