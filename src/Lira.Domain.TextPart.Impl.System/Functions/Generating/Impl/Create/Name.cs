namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Name_ : FunctionBase, IObjectTextPart
{
    public override string Name => "name";

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return NameFirst.Next();
        yield return " ";
        yield return NameLast.Next();
    }

    public ReturnType ReturnType => ReturnType.String;
}
