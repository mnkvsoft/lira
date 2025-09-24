namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public class ObjectTextPartWithSaveVariable(IObjectTextPart objectTextPart, Variable variable) : IObjectTextPart
{
    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var value = objectTextPart.Generate(context);
        variable.SetValue(context, value);
        yield return value;
    }

    public ReturnType? ReturnType => objectTextPart.ReturnType;
}