namespace Lira.Domain.TextPart.Impl.System.Functions;

abstract class DicBase(ICustomDictsProvider customDictsProviderProvider) : WithArgumentFunction<string>
{
    public override bool ArgumentIsRequired => true;
    protected CustomDic CustomDic { get; private set; } = null!;

    public override void SetArgument(string arguments)
    {
        CustomDic = customDictsProviderProvider.GetCustomDic(name: arguments);
    }
}