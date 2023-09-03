namespace SimpleMockServer.Domain.TextPart.Impl.Custom;

public static class FunctionCollectionExtensions
{
    public static Function GetOrThrow(this IReadOnlyCollection<Function> functions, string name)
    {
        var result = functions.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Variable '{name}' not found");
        return result;
    }
}