using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;

public class LocalVariable : Variable
{
    public const string Prefix = "$";
    public static readonly NamingStrategy NamingStrategy = CreateNamingStrategy(Prefix);

    private static int _counter;
    private readonly int _id;
    public override string Name { get; }
    public override Type Type { get; }
    private readonly TypeInfo _typeInfo;

    public LocalVariable(string name, TypeInfo typeInfo)
    {
        if(!IsValidName(name))
            throw new ArgumentException("Invalid local variable name: " + name, nameof(name));

        Name = name;
        Type = typeInfo.TargetType;
        _id = Interlocked.Increment(ref _counter);
        _typeInfo = typeInfo;
    }

    private static readonly object NullValue = new();

    public static bool IsValidName(string name) => NamingStrategy.IsValidName(name);

    public override dynamic? Get(RuleExecutingContext ctx)
    {
        var values = GetVariableValues(ctx);

        if (values.TryGetValue(_id, out var value))
            return ReferenceEquals(value, NullValue) ? null : value;

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

    private static Dictionary<int, dynamic?> GetVariableValues(RuleExecutingContext ctx)
    {
        return ctx.Items.GetOrCreate(key: typeof(LocalVariable), () => new Dictionary<int, dynamic?>());
    }
}