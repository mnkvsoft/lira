using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public abstract record Variable : IUniqueSetItem, IObjectTextPart
{
    private readonly CustomItemName _name;

    public string Name => _name.Value;
    public string EntityName => "variable";

    private readonly ReturnType? _type;

    protected Variable(CustomItemName name, ReturnType? type)
    {
        _name = name;
        _type = type;
    }

    private static readonly object NullValue = new();
    protected abstract Task<dynamic?> GetInitiatedValue(RuleExecutingContext ctx);

    public async Task<dynamic?> GetValue(RuleExecutingContext ctx)
    {
        var values = GetVariableValues(ctx);

        if (values.TryGetValue(_name, out var value))
            return ReferenceEquals(value, NullValue) ? null : value;

        var initValue = await GetInitiatedValue(ctx);
        SetValue(ctx, initValue);
        return initValue;
    }

    public void SetValue(RuleExecutingContext ctx, dynamic? value)
    {
        dynamic? valueToSave = value;

        if (_type != null)
        {
            if (!TypedValueCreator.TryCreate(_type, value, out dynamic? valueTyped, out Exception exc))
            {
                throw new Exception(
                    $"Can't cast value '{value}' " +
                    $"of type '{value?.GetType()}' " +
                    $"to type '{_type}'" +
                    $"for write to variable '{Name}'",
                    exc);
            }

            valueToSave = valueTyped;
        }

        var values = GetVariableValues(ctx);

        if (valueToSave == null)
            values[_name] = NullValue;
        else
            values[_name] = valueToSave;
    }

    private static Dictionary<CustomItemName, dynamic?> GetVariableValues(RuleExecutingContext ctx)
    {
        return ctx.Items.GetOrCreate(key: typeof(Variable), () => new Dictionary<CustomItemName, dynamic?>());
    }

    Task<dynamic?> IObjectTextPart.Get(RuleExecutingContext context) => GetValue(context);
}