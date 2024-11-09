namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Name_ : FunctionBase, IObjectTextPart
{
    public override string Name => "name";

    public Task<dynamic?> Get(RuleExecutingContext context)
    {
        return Task.FromResult<dynamic?>(NameFirst.Next() + " " + NameLast.Next());
    }
}
