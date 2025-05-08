namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public interface IVariable
{
    void SetValue(RuleExecutingContext ctx, dynamic? value);
}