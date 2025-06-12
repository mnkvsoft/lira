using Lira.Domain.DataModel;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;
internal class Range : RangeBase, IObjectTextPart
{
    public Range(IRangesProvider dataProvider) : base(dataProvider)
    {
    }

    public override string Name => "range";
    public override bool ArgumentIsRequired => true;
    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return GetRange().NextValue().ToString();
    }

    public ReturnType ReturnType => ReturnType.String;
}
