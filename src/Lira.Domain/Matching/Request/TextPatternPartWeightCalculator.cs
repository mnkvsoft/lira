using Lira.Common.Exceptions;

namespace Lira.Domain.Matching.Request;

public static class WeightValue
{
    public const int StaticFull = 10;
    public const int StaticPart = 1;

    public static class Function
    {
        public const int Any = 3;
        public const int TypeRestriction = 4;
        public const int RangeRestriction = 5;

        // must be more than StaticPart(1) + RangeRestriction(5) + StaticPart(1)
        // 1{{ range: some }}2 (7)
        // {{ some custom c# code or regex }} (8)
        public const int CustomRestriction = 8;
    }

    // because conditions are triggered within one rule, then simply the more conditions, the greater the weight
    public const int Condition = 1;
    public const int CustomCode = 1;
}

static class TextPatternPartWeightCalculator
{
    public static int Calculate(TextPatternPart part)
    {
        if (part is TextPatternPart.Static or TextPatternPart.NullOrEmpty)
            return WeightValue.StaticFull;

        if (part is TextPatternPart.Dynamic dynamicPart)
        {
            int result = 0;

            if (!string.IsNullOrEmpty(dynamicPart.Start))
                result += WeightValue.StaticPart;

            if (!string.IsNullOrEmpty(dynamicPart.End))
                result += WeightValue.StaticPart;

            result += GetFunctionWeight(dynamicPart.MatchFunction.Restriction);

            return result;
        }

        throw new UnsupportedInstanceType(part);
    }

    private static int GetFunctionWeight(MatchFunctionRestriction restriction)
    {
        switch (restriction)
        {
            case MatchFunctionRestriction.Any: return WeightValue.Function.Any;
            case MatchFunctionRestriction.Type: return WeightValue.Function.TypeRestriction;
            case MatchFunctionRestriction.Range: return WeightValue.Function.RangeRestriction;
            case MatchFunctionRestriction.Custom: return WeightValue.Function.CustomRestriction;
            default:
                throw new UnsupportedEnumValue(restriction);
        }
    }
}
