namespace Lira.Domain.Matching.Request;

static class MatchedValuesSetExtensions
{
    public static void AddIfValueIdNotNull(this IDictionary<string, string?> set, TextPatternPart.MatchResult.Matched matched)
    {
        if (matched.DynamicValueId != null)
        {
            set.Add(matched.DynamicValueId, matched.DynamicValue);
        }
    }
}
