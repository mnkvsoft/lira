using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;

public class LocalVariable : Variable
{
    public const string Prefix = "$";

    private static int _counter;
    private readonly int _id;
    public override string Name { get; }
    public override ReturnType? ReturnType { get; }

    public LocalVariable(string name, ReturnType? valueType)
    {
        if(!CustomItemName.IsValidName(Prefix, name))
            throw new ArgumentException("Invalid local variable name: " + name, nameof(name));

        Name = name;
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

    public override void SetValue(RuleExecutingContext ctx, dynamic? valueToSave)
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

    public override Task<dynamic?> Get(RuleExecutingContext context) => GetValue(context);
}