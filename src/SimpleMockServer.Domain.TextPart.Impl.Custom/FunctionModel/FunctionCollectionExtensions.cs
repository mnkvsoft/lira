namespace SimpleMockServer.Domain.TextPart.Impl.Custom.FunctionModel;

public static class FunctionCollectionExtensions
{
    public static Function GetOrThrow(this IReadOnlyCollection<Function> functions, string name)
    {
        var result = functions.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Function '{name}' not found. Available: {string.Join(", ", functions.Select(v => $"'{ v.Name}'"))}");
        return result;
    }
}