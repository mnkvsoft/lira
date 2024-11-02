using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public record Variable : IObjectTextPart, IUniqueSetItem
{
    private readonly CustomItemName _name;
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;
    private static readonly object NullValue = new();

    public string Name => _name.Value;
    public string EntityName => "variable";

    public Variable(CustomItemName name, IReadOnlyCollection<IObjectTextPart> parts)
    {
        _parts = parts;
        _name = name;
    }

    public dynamic? Get(RuleExecutingContext context)
    {
        var nameToValue = context.Items.TryGetValueOrCreate(typeof(Variable), () => new Dictionary<CustomItemName, object>());
        if (nameToValue.TryGetValue(_name, out var value))
        {
            if (value == NullValue)
                return null;

            return value;
        }

        object? newValue = _parts.Generate(context);
        nameToValue.Add(_name, newValue ?? NullValue);
        return newValue;
    }
}
