namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.Impl;

public record DeclaredVariable : Variable
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;

    public DeclaredVariable(CustomItemName name, IReadOnlyCollection<IObjectTextPart> parts, ReturnType? type) : base(name, type)
    {
        _parts = parts;
    }
    protected override Task<dynamic?> GetInitiatedValue(RuleExecutingContext ctx) => _parts.Generate(ctx);
}