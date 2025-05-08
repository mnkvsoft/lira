using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;

public record LocalVariable : DeclaredItem, IUniqueSetItem, IObjectTextPart, IVariable
{
    private static int _counter;
    private readonly int _id;
    private readonly CustomItemName _name;

    public override string Name => _name.Value;
    public string EntityName => "local variable";

    public ReturnType? ReturnType { get; }

    public LocalVariable(CustomItemName name, ReturnType? valueType)
    {
        _name = name;
        ReturnType = valueType;
        _id = Interlocked.Increment(ref _counter);
    }

    private static readonly object NullValue = new();

    private Task<dynamic?> GetValue(RuleExecutingContext ctx)
    {
        var values = GetVariableValues(ctx);

        if (values.TryGetValue(_id, out var value))
            return Task.FromResult<dynamic?>(ReferenceEquals(value, NullValue) ? null : value);

        throw new InvalidOperationException($"Attempt to read from uninitialized variable '{Name}'");
    }

    public void SetValue(RuleExecutingContext ctx, dynamic? valueToSave)
    {
        var values = GetVariableValues(ctx);

        if (valueToSave == null)
            values[_id] = NullValue;
        else
            values[_id] = GetValueTyped(valueToSave);
    }

    private dynamic? GetValueTyped(dynamic? value)
    {
        if (ReturnType == null)
            return value;

        if (!TypedValueCreator.TryCreate(ReturnType, value, out dynamic? valueTyped, out Exception exc))
        {
            throw new Exception(
                $"Can't cast value '{value}' " +
                $"of type '{value?.GetType()}' " +
                $"to type '{ReturnType}'" +
                $"for write to variable '{Name}'",
                exc);
        }

        return valueTyped;
    }

    private static Dictionary<int, dynamic?> GetVariableValues(RuleExecutingContext ctx)
    {
        return ctx.Items.GetOrCreate(key: typeof(LocalVariable), () => new Dictionary<int, dynamic?>());
    }

    Task<dynamic?> IObjectTextPart.Get(RuleExecutingContext context) => GetValue(context);
}