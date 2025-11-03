using Lira.Common;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Int : Number<long>
{
    public override string Name => "int";
    public override Type ValueType => ExplicitType.Int.DotnetType;
    protected override bool TryParse(string? value, out long result) => long.TryParse(value, out result);
}

internal class Dec : Number<double>
{
    public override string Name => "dec";
    public override Type ValueType => ExplicitType.Decimal.DotnetType;
    protected override bool TryParse(string? value, out double result) => double.TryParse(value, out result);
}

internal abstract class Number<T> : WithRangeArgumentFunction<T>, IMatchFunctionTyped where T : struct, IComparable<T>
{
    public override bool ArgumentIsRequired => false;
    public MatchFunctionRestriction Restriction => _range == null ? MatchFunctionRestriction.Type : MatchFunctionRestriction.Range;
    public abstract Type ValueType { get; }

    private Interval<T>? _range;

    protected abstract bool TryParse(string? value, out T result);

    public bool IsMatch(RuleExecutingContext context, string? value) => IsMatchTyped(context, value, out _);

    public bool IsMatchTyped(RuleExecutingContext context, string? value, out dynamic? typedValue)
    {
        typedValue = null;

        if (!TryParse(value, out T number))
            return false;

        if (_range == null || _range.InRange(number))
        {
            typedValue = number;
            return true;
        }

        return false;
    }

    public override void SetArgument(Interval<T> argument)
    {
        _range = argument;
    }
}