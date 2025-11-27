using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Dic(ICustomDictsProvider customDictsProviderProvider) : DicBase(customDictsProviderProvider), IMatchFunctionTyped
{
    public override string Name => "dic";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;
    public ReturnType ValueType => ReturnType.String;

    public Task<bool> IsMatch(RuleExecutingContext _, string? value)
    {
        if (value is null)
            return Task.FromResult(false);

        return Task.FromResult(CustomDic.ValueIsBelong(value));
    }
}
