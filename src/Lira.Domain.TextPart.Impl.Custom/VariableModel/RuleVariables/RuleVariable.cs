using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

public abstract class RuleVariable : Variable
{
    public const string Prefix = "$$";

    public static readonly NamingStrategy NamingStrategy = CreateNamingStrategy(Prefix);

    public override string Name { get; }
    public override Type Type { get; }

    private readonly TypeInfo _typeInfo;

    protected RuleVariable(string name, TypeInfo typeInfo)
    {
        if (!IsValidName(name))
            throw new ArgumentException("Invalid rule variable name: " + name, nameof(name));

        Name = name;
        Type = typeInfo.TargetType;
        _typeInfo = typeInfo;
    }

    public static bool IsValidName(string name) => NamingStrategy.IsValidName(name);

    private static readonly object NullValue = new();
    protected abstract dynamic? GetInitiatedValue(RuleExecutingContext ctx);

    public override dynamic? Get(RuleExecutingContext ctx)
    {
        var values = GetVariableValues(ctx);

        if (values.TryGetValue(Name, out var value))
            return ReferenceEquals(value, NullValue) ? null : value;

        var initValue = GetValueTyped(GetInitiatedValue(ctx));
        SetValueInternal(ctx, initValue);
        return initValue;
    }

    public override void SetValue(RuleExecutingContext ctx, dynamic? value)
    {
        SetValueInternal(ctx, GetValueTyped(value));
    }

    private void SetValueInternal(RuleExecutingContext ctx, dynamic valueToSave)
    {
        var values = GetVariableValues(ctx);

        if (valueToSave == null)
            values[Name] = NullValue;
        else
            values[Name] = valueToSave;
    }

    private dynamic? GetValueTyped(dynamic? value)
    {
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

    private static Dictionary<string, dynamic?> GetVariableValues(RuleExecutingContext ctx)
    {
        return ctx.Items.GetOrCreate(key: typeof(RuleVariable), () => new Dictionary<string, dynamic?>());
    }
}