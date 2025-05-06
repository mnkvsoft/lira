namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.Impl;

public record RuntimeVariable : Variable
{
    public RuntimeVariable(CustomItemName name, ReturnType? valueType) : base(name, valueType)
    {
    }

    protected override Task<dynamic?> GetInitiatedValue(RuleExecutingContext ctx) => throw new InvalidOperationException($"Attempt to read from uninitialized variable '{Name}'");
}