using Lira.Common;

namespace Lira.Domain.TextPart.Impl.Custom.FunctionModel;

public record Function : DeclaredItem,  IObjectTextPart, IUniqueSetItem
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;
    public ReturnType? ReturnType { get; }

    private readonly CustomItemName _name;
    public override string Name => _name.Value;
    public string EntityName => "function";

    public Function(CustomItemName name, IReadOnlyCollection<IObjectTextPart> parts, ReturnType? valueType)
    {
        _parts = parts;
        ReturnType = valueType;
        _name = name;
    }

    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        var value = await _parts.Generate(context);
        dynamic? valueToReturn = value;

        if (ReturnType != null)
        {
            if (!TypedValueCreator.TryCreate(ReturnType, value, out dynamic? valueTyped, out Exception exc))
            {
                throw new Exception(
                    $"Can't cast value '{value}' " +
                    $"of type '{value?.GetType()}' " +
                    $"to type '{ReturnType}'" +
                    $"for return from function '{Name}'",
                    exc);
            }

            valueToReturn = valueTyped;
        }

        return valueToReturn;
    }
}