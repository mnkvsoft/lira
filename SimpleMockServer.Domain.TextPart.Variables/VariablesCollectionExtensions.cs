namespace SimpleMockServer.Domain.TextPart.Variables;

public static class VariablesCollectionExtensions
{
    public static TVariable GetOrThrow<TVariable>(this IReadOnlyCollection<TVariable> variables, string name) where TVariable : Variable
    {
        var result = variables.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Variable '{name}' not declared");
        return result;
    }

    public static VariableSet<TVariable> AddRange<TVariable>(this VariableSet<TVariable> set, IReadOnlyCollection<TVariable> variables) where TVariable : Variable
    {
        foreach( var v in variables )
        {
            set.Add(v);
        }
        return set;
    }
}

