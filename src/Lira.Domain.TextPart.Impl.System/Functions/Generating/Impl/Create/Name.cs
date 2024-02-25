namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Name_ : FunctionBase, IObjectTextPart
{
    public override string Name => "name";

    public dynamic? Get(RuleExecutingContext context)
    {
        return NameFirst.Next() + " " + NameLast.Next();
    }
}
