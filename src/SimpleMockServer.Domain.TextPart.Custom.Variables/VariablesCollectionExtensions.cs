namespace SimpleMockServer.Domain.TextPart.Custom.Variables;

public static class VariablesCollectionExtensions
{
    public static Variable GetOrThrow(this IReadOnlyCollection<Variable> vars, string name)
    {
        var result = vars.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Variable '{name}' not found");
        return result;
    }
}