using Lira.Common;

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

    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        var key = "variable_" + _name;
        if (context.Items.TryGetValue(key, out var value))
        {
            if (value == NullValue)
                return null;

            return value;
        }

        object? newValue = await _parts.Generate(context);
        context.Items.Add(key, newValue ?? NullValue);
        return newValue;
    }
}
