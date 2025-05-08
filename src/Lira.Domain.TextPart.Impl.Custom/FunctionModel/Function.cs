namespace Lira.Domain.TextPart.Impl.Custom.FunctionModel;

public class Function : DeclaredItem
{
    public const string Prefix = "#";

    private readonly IReadOnlyCollection<IObjectTextPart> _parts;
    public override ReturnType? ReturnType { get; }

    public override string Name { get; }

    public Function(string name, IReadOnlyCollection<IObjectTextPart> parts, ReturnType? valueType)
    {
        if(!CustomItemName.IsValidName(Prefix, name))
            throw new ArgumentException("Invalid function name: " + name, nameof(name));

        _parts = parts;
        ReturnType = valueType;
        Name = name;
    }

    public override async Task<dynamic?> Get(RuleExecutingContext context)
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