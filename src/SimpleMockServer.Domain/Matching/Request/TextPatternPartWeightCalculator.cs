using SimpleMockServer.Common.Exceptions;

namespace SimpleMockServer.Domain.Matching.Request;

static class WeightValue
{
    public const int StaticFull = 10;
    public const int StaticPart = 1;
    public static class Function
    {
        public const int TypeRestriction = 4;
        public const int RangeRestriction = 5;
        public const int Any = 3;
    }
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
            default:
                throw new UnsupportedEnumValue(restriction);
        }
    }
}
