using Lira.Common;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Int : Number<long>
{
    public override string Name => "int";
    public override ReturnType ValueType => ReturnType.Int;
    protected override bool TryParse(string? value, out long result) => long.TryParse(value, out result);
}

internal class Dec : Number<decimal>
{
    public override string Name => "dec";
    public override ReturnType ValueType => ReturnType.Decimal;
    protected override bool TryParse(string? value, out decimal result) => decimal.TryParse(value, out result);
}

internal abstract class Number<T> : WithRangeArgumentFunction<T>, IMatchFunctionTyped where T : struct, IComparable<T>
{
    public override bool ArgumentIsRequired => false;
    public MatchFunctionRestriction Restriction => _range == null ? MatchFunctionRestriction.Type : MatchFunctionRestriction.Range;
    public abstract ReturnType ValueType { get; }

    private Interval<T>? _range;

    protected abstract bool TryParse(string? value, out T result);

    public Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        if (!TryParse(value, out T number))
            return Task.FromResult(false);

        if (_range == null)
            return Task.FromResult(true);

        return Task.FromResult(_range.InRange(number));
    }

    public override void SetArgument(Interval<T> argument)
    {
        _range = argument;
    }
}