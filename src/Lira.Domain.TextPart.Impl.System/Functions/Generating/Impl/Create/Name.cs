namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Name_ : FunctionBase, IObjectTextPart
{
    public override string Name => "name";

    public dynamic Get(RuleExecutingContext _) => GetInternal();

    private IEnumerable<dynamic> GetInternal()
    {
        yield return NameFirst.Next();
        yield return " ";
        yield return NameLast.Next();
    }

    public Type Type => DotNetType.EnumerableDynamic;
}
