namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public class ObjectTextPartWithSaveVariable(IObjectTextPart objectTextPart, Variable variable) : IObjectTextPart
{
    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var value = await objectTextPart.Generate(context);
        variable.SetValue(context, value);
        yield return  value;
    }

    public ReturnType? ReturnType => objectTextPart.ReturnType;
}