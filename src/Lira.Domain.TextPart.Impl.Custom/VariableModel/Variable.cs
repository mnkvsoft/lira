using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public abstract record Variable : IUniqueSetItem, IObjectTextPart
{
    private readonly CustomItemName _name;

    public string Name => _name.Value;
    public string EntityName => "variable";

    public readonly ReturnType? Type;

    protected Variable(CustomItemName name, ReturnType? type)
    {
        _name = name;
        Type = type;
    }

    private static readonly object NullValue = new();
    protected abstract Task<dynamic?> GetInitiatedValue(RuleExecutingContext ctx);

    public async Task<dynamic?> GetValue(RuleExecutingContext ctx)
    {
        var values = GetVariableValues(ctx);

        if (values.TryGetValue(_name, out var value))
            return ReferenceEquals(value, NullValue) ? null : value;

        var initValue = GetValueTyped(await GetInitiatedValue(ctx));
        SetValueInternal(ctx, initValue);
        return initValue;
    }

    public void SetValue(RuleExecutingContext ctx, dynamic? value)
    {
        SetValueInternal(ctx, GetValueTyped(value));
    }

    private void SetValueInternal(RuleExecutingContext ctx, dynamic valueToSave)
    {
        var values = GetVariableValues(ctx);

        if (valueToSave == null)
            values[_name] = NullValue;
        else
            values[_name] = valueToSave;
    }

    private dynamic? GetValueTyped(dynamic? value)
    {
        if (Type == null)
            return value;

        if (!TypedValueCreator.TryCreate(Type, value, out dynamic? valueTyped, out Exception exc))
        {
            throw new Exception(
                $"Can't cast value '{value}' " +
                $"of type '{value?.GetType()}' " +
                $"to type '{Type}'" +
                $"for write to variable '{Name}'",
                exc);
        }

        return valueTyped;
    }

    private static Dictionary<CustomItemName, dynamic?> GetVariableValues(RuleExecutingContext ctx)
    {
        return ctx.Items.GetOrCreate(key: typeof(Variable), () => new Dictionary<CustomItemName, dynamic?>());
    }

    Task<dynamic?> IObjectTextPart.Get(RuleExecutingContext context) => GetValue(context);
}