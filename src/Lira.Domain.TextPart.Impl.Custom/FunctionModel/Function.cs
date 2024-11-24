using Lira.Common;

namespace Lira.Domain.TextPart.Impl.Custom.FunctionModel;

public record Function : IObjectTextPart, IUniqueSetItem
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;
    private readonly ReturnType? _type;

    private readonly CustomItemName _name;
    public string Name => _name.Value;
    public string EntityName => "function";

    public Function(CustomItemName name, IReadOnlyCollection<IObjectTextPart> parts, ReturnType? type)
    {
        _parts = parts;
        _type = type;
        _name = name;
    }

    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        var value = await _parts.Generate(context);
        dynamic? valueToReturn = value;

        if (_type != null)
        {
            if (!TypedValueCreator.TryCreate(_type, value, out dynamic? valueTyped, out Exception exc))
            {
                throw new Exception(
                    $"Can't cast value '{value}' " +
                    $"of type '{value?.GetType()}' " +
                    $"to type '{_type}'" +
                    $"for return from function '{Name}'",
                    exc);
            }

            valueToReturn = valueTyped;
        }

        return valueToReturn;
    }
}