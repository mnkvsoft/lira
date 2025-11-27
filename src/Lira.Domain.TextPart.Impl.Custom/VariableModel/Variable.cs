namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public abstract class Variable : DeclaredItem
{
    public abstract void SetValue(RuleExecutingContext ctx, dynamic? value);

    protected static NamingStrategy CreateNamingStrategy(string prefix) => new(
        prefix,
        IsAllowedFirstChar: c => char.IsLetter(c) || c == '_',
        IsAllowedChar: c => char.IsLetter(c) || char.IsDigit(c) || c == '_');
}