namespace Lira.Domain.TextPart.Impl.Custom.FunctionModel;

public class Function : DeclaredItem
{
    public const string Prefix = "@";

    public static readonly NamingStrategy NamingStrategy = new(
        Prefix,
        IsAllowedFirstChar: c => char.IsLetter(c) || c == '_',
        IsAllowedChar: c => char.IsLetter(c) || char.IsDigit(c) || c == '_' || c == '.');

    private readonly IObjectTextPart _part;
    private readonly TypeInfo _typeInfo;
    public override Type Type { get; }

    public override string Name { get; }

    public Function(string name, IObjectTextPart part, TypeInfo typeInfo)
    {
        if(!IsValidName(name))
            throw new ArgumentException("Invalid function name: " + name, nameof(name));

        _part = part;
        _typeInfo = typeInfo;
        Type = typeInfo.TargetType;
        Name = name;
    }

    public static bool IsValidName(string name) => NamingStrategy.IsValidName(name);

    public override dynamic? Get(RuleExecutingContext context)
    {
        var value = _part.Get(context);

        if (!_typeInfo.TryGet(value, out dynamic? result, out Exception exc))
        {
            throw new Exception(
                $"Can't explicitly cast value '{value}' " +
                $"of type '{value?.GetType()}' " +
                $"to type '{Type}'" +
                $"for write to variable '{Name}'",
                exc);
        }

        return result;
    }
}