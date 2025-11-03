namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public class ObjectTextPartWithSaveVariable(IObjectTextPart objectTextPart, Variable variable) : IObjectTextPart
{
    public dynamic? Get(RuleExecutingContext context)
    {
        var value = objectTextPart.Generate(context);
        variable.SetValue(context, value);
        return value;
    }

    public Type Type => objectTextPart.Type;
}