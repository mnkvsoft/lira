namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public abstract class Variable : DeclaredItem
{
    public abstract void SetValue(RuleExecutingContext ctx, dynamic? value);
}

// runtime => inline
public interface IInlineDeclareVariable : IObjectTextPart
{
    void SetValue(RuleExecutingContext ctx, dynamic? value);
}