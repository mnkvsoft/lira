namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public interface IVariablesProvider
{
    Task<dynamic?> GetVariable(RuleExecutingContext ctx, CustomItemName name);
    void SetVariable(RuleExecutingContext ctx, CustomItemName name, dynamic? value);
}

public class VariablesProvider : IVariablesProvider
{
    static readonly Type ItemsKey = typeof(VariablesProvider);
    private readonly IReadOnlyDictionary<CustomItemName, DeclaredVariable> _declaredVariables;
    private static readonly object NullValue = new();

    public VariablesProvider(IReadOnlyDictionary<CustomItemName, DeclaredVariable> declaredVariables)
    {
        _declaredVariables = declaredVariables;
    }

    public async Task<dynamic?> GetVariable(RuleExecutingContext ctx, CustomItemName name)
    {
        var variables = GetVariablesDict(ctx);

        if (variables.TryGetValue(name, out var variable))
        {
            return ReferenceEquals(variable, NullValue) ? null : variable;
        }

        if (!_declaredVariables.TryGetValue(name, out var declaredVariable))
        {
            throw new Exception($"Variable '{name}' is not declared.");
        }

        var value = await declaredVariable.Get(ctx);
        SetVariable(ctx, name, value);;
        return value;
    }

    public void SetVariable(RuleExecutingContext ctx, CustomItemName name, dynamic? value)
    {
        var variables = GetVariablesDict(ctx);
        SetVariable(variables, name, value);
    }

    private static Dictionary<CustomItemName, dynamic?> GetVariablesDict(RuleExecutingContext ctx)
    {
        Dictionary<CustomItemName, dynamic?> variables;

        if (ctx.Items.TryGetValue(ItemsKey, out var variablesObj))
        {
            variables = (Dictionary<CustomItemName, dynamic?>)variablesObj!;
        }
        else
        {
            variables = new Dictionary<CustomItemName, dynamic?>();
            ctx.Items.Add(ItemsKey, variables);
        }

        return variables;
    }

    private static void SetVariable(IDictionary<CustomItemName, dynamic?> variables, CustomItemName name, dynamic? value)
    {
        if (value == null)
        {
            variables[name] = NullValue;
        }
        else
        {
            variables[name] = value;
        }
    }
}