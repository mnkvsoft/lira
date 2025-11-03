namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;
internal class Dic(ICustomDictsProvider customDictsProviderProvider) :  DicBase(customDictsProviderProvider), IObjectTextPart
{
    public override string Name => "dic";
    public override bool ArgumentIsRequired => true;
    public Type Type => DotNetType.String;

    public dynamic Get(RuleExecutingContext context)
    {
        return CustomDic.NextValue();
    }
}
