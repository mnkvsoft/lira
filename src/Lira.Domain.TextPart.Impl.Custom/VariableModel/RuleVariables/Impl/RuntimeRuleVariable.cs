namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;

public record RuntimeRuleVariable : RuleVariable
{
    public RuntimeRuleVariable(CustomItemName name, ReturnType? valueType) : base(name, valueType)
    {
    }

    protected override Task<dynamic?> GetInitiatedValue(RuleExecutingContext ctx) => throw new InvalidOperationException($"Attempt to read from uninitialized variable '{Name}'");
}