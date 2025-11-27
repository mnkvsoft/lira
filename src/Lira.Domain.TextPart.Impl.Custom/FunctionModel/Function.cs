namespace Lira.Domain.TextPart.Impl.Custom.FunctionModel;

public class Function : DeclaredItem
{
    public const string Prefix = "@";

    public static readonly NamingStrategy NamingStrategy = new(
        Prefix,
        IsAllowedFirstChar: c => char.IsLetter(c) || c == '_',
        IsAllowedChar: c => char.IsLetter(c) || char.IsDigit(c) || c == '_' || c == '.');

    private readonly IReadOnlyCollection<IObjectTextPart> _parts;
    public override ReturnType? ReturnType { get; }

    public override string Name { get; }

    public Function(string name, IReadOnlyCollection<IObjectTextPart> parts, ReturnType? valueType)
    {
        if(!IsValidName(name))
            throw new ArgumentException("Invalid function name: " + name, nameof(name));

        _parts = parts;
        ReturnType = valueType;
        Name = name;
    }

    public static bool IsValidName(string name) => NamingStrategy.IsValidName(name);

    public override IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var value = _parts.Generate(context);
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

        yield return valueToReturn;
    }
}