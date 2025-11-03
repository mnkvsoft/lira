namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;

public class DeclaredRuleVariable : RuleVariable
{
    private readonly IObjectTextPart _part;

    public DeclaredRuleVariable(string name, IObjectTextPart part, TypeInfo typeInfo) : base(name, typeInfo)
    {
        _part = part;
    }

    protected override dynamic? GetInitiatedValue(RuleExecutingContext ctx) => _part.Get(ctx);
}