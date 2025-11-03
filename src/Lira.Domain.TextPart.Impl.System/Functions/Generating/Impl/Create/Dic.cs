namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;
internal class Dic(ICustomDictsProvider customDictsProviderProvider) :  DicBase(customDictsProviderProvider), IObjectTextPart
{
    public override string Name => "dic";
    public override bool ArgumentIsRequired => true;
    public ReturnType ReturnType => ReturnType.String;

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return CustomDic.NextValue();
    }
}
