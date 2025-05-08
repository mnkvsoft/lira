namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;

public record DeclaredRuleVariable : RuleVariable
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;

    public DeclaredRuleVariable(CustomItemName name, IReadOnlyCollection<IObjectTextPart> parts, ReturnType? valueType) : base(name, valueType)
    {
        _parts = parts;
    }
    protected override Task<dynamic?> GetInitiatedValue(RuleExecutingContext ctx) => _parts.Generate(ctx);
}