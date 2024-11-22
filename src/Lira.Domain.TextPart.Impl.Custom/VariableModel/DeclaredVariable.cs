using Lira.Common;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public record DeclaredVariable : IObjectTextPart, IUniqueSetItem
{
    private readonly CustomItemName _name;
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;

    public string Name => _name.Value;
    public string EntityName => "variable";

    public DeclaredVariable(CustomItemName name, IReadOnlyCollection<IObjectTextPart> parts)
    {
        _parts = parts;
        _name = name;
    }

    public Task<dynamic?> Get(RuleExecutingContext context) => _parts.Generate(context);
}