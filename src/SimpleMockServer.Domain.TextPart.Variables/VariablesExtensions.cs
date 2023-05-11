namespace SimpleMockServer.Domain.TextPart.Variables;

public static class VariablesExtensions
{
    public static IReadOnlyCollection<Variable> Combine(this IReadOnlyCollection<Variable> set1, IReadOnlyCollection<Variable> set2)
    {
        var result = new VariableSet(set1);
        result.AddRange(set2);
        return result;
    }
}
