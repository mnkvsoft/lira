using Lira.Common.Extensions;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

public abstract class RuleVariable : Variable
{
    public const string Prefix = "$$";

    public override string Name { get; }
    public override ReturnType? ReturnType { get; }

    protected RuleVariable(string name, ReturnType? valueType)
    {
        if(!IsValidName(name))
            throw new ArgumentException("Invalid local variable name: " + name, nameof(name));

        Name = name;
        ReturnType = valueType;
    }

    public static bool IsValidName(string name) => CustomItemName.IsValidName(Prefix, name);

    private static readonly object NullValue = new();
    protected abstract ValueTask<dynamic?> GetInitiatedValue(RuleExecutingContext ctx);

    private async ValueTask<dynamic?> GetValue(RuleExecutingContext ctx)
    {
        var values = GetVariableValues(ctx);

        if (values.TryGetValue(Name, out var value))
            return ReferenceEquals(value, NullValue) ? null : value;

        var initValue = GetValueTyped(await GetInitiatedValue(ctx));
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

    private static Dictionary<string, dynamic?> GetVariableValues(RuleExecutingContext ctx)
    {
        return ctx.Items.GetOrCreate(key: typeof(RuleVariable), () => new Dictionary<string, dynamic?>());
    }

    public override async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var value = await GetValue(context);
        yield return value;
    }
}