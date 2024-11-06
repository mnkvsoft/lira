using Lira.Common;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Int : Number<long>
{
    public override string Name => "int";
    protected override bool TryParse(string? value, out long result) => long.TryParse(value, out result);
}

internal class Float : Number<decimal>
{
    public override string Name => "float";
    protected override bool TryParse(string? value, out decimal result) => decimal.TryParse(value, out result);
}

internal abstract class Number<T> : WithRangeArgumentFunction<T>, IMatchFunctionSystem where T : struct, IComparable<T>
{
    public override bool ArgumentIsRequired => false;
    public MatchFunctionRestriction Restriction => _range == null ? MatchFunctionRestriction.Type : MatchFunctionRestriction.Range;

    private Interval<T>? _range;

    protected abstract bool TryParse(string? value, out T result);

    public bool IsMatch(RuleExecutingContext context, string? value)
    {
        if (!TryParse(value, out T number))
            return false;

        if (_range == null)
            return true;

        return _range.InRange(number);
    }

    public override void SetArgument(Interval<T> argument)
    {
        _range = argument;
    }
}