using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

public static class VariablesCollectionExtensions
{
    public static RuleVariable GetOrThrow(this IReadOnlyCollection<RuleVariable> vars, string name)
    {
        var result = vars.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Variable '{name}' not found. Available: {string.Join(", ", vars.Select(v => $"'{ v.Name}'"))}");
        return result;
    }

    public static LocalVariable GetOrThrow(this IReadOnlyCollection<LocalVariable> vars, string name)
    {
        var result = vars.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Local variable '{name}' not found. Available: {string.Join(", ", vars.Select(v => $"'{ v.Name}'"))}");
        return result;
    }
}