namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Name_ : FunctionBase, IObjectTextPart
{
    public override string Name => "name";

    public object Get(RequestData request)
    {
        return NameFirst.Next() + " " + NameLast.Next();
    }
}
