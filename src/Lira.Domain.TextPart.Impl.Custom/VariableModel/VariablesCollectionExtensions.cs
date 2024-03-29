﻿namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public static class VariablesCollectionExtensions
{
    public static Variable GetOrThrow(this IReadOnlyCollection<Variable> vars, string name)
    {
        var result = vars.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Variable '{name}' not found. Available: {string.Join(", ", vars.Select(v => $"'{ v.Name}'"))}");
        return result;
    }
}