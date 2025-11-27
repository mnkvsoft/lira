namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;

public class RuntimeRuleVariable : RuleVariable
{
    public RuntimeRuleVariable(string name, ReturnType? valueType) : base(name, valueType)
    {
    }

    protected override dynamic GetInitiatedValue(RuleExecutingContext ctx) => throw new InvalidOperationException($"Attempt to read from uninitialized variable '{Name}'");
}