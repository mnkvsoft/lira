using Lira.Common;

namespace Lira.Domain.TextPart.Impl.Custom.FunctionModel;

public record Function : CustomItem,  IObjectTextPart, IUniqueSetItem
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;
    public readonly ReturnType? Type;

    private readonly CustomItemName _name;
    public override string Name => _name.Value;
    public string EntityName => "function";

    public Function(CustomItemName name, IReadOnlyCollection<IObjectTextPart> parts, ReturnType? type)
    {
        _parts = parts;
        Type = type;
        _name = name;
    }

    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        var value = await _parts.Generate(context);
        dynamic? valueToReturn = value;

        if (Type != null)
        {
            if (!TypedValueCreator.TryCreate(Type, value, out dynamic? valueTyped, out Exception exc))
            {
                throw new Exception(
                    $"Can't cast value '{value}' " +
                    $"of type '{value?.GetType()}' " +
                    $"to type '{Type}'" +
                    $"for return from function '{Name}'",
                    exc);
            }

            valueToReturn = valueTyped;
        }

        return valueToReturn;
    }
}