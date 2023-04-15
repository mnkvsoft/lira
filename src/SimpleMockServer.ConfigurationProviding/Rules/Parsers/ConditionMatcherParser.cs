using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules.Parsers;

class ConditionMatcherParser
{
    private readonly IRequestStatisticStorage _requestStatisticStorage;

    public ConditionMatcherParser(IRequestStatisticStorage requestStatisticStorage)
    {
        _requestStatisticStorage = requestStatisticStorage;
    }

    static class ConditionMatcherName
    {
        public const string Attempt = "$attempt";
    }

    public ConditionMatcherSet Parse(FileSection conditionSection)
    {
        return new ConditionMatcherSet(_requestStatisticStorage, conditionSection.LinesWithoutBlock.Select(CreateConditionMatcher).ToArray());
    }

    private IConditionMatcher CreateConditionMatcher(string line)
    {
        var splitResult = line.SplitBy(">=", "<=", "=", ">", "<")?.Trim();

        if (splitResult == null)
            throw new ArgumentException($"Cannot parse line '{line}'");

        if(splitResult.LeftPart == ConditionMatcherName.Attempt)
        {
            if (!int.TryParse(splitResult.RightPart, out int value))
                throw new ArgumentException($"Not int value '{splitResult.RightPart}' in line {line}");

            var comparer = GetIntComparer(splitResult.Splitter, value);

            return new AttemptConditionMatcher(new ComparableMatchFunction<int>(comparer));
        }

        throw new Exception($"Unknown variable in condition section: '{splitResult.LeftPart}'");
    }

    private Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt.Comparer<int> GetIntComparer(string splitter, int value)
    {
        switch (splitter)
        {
            case "=":
                return Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt.Comparer<int>.AreEquals(value);
            case "<":
                return Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt.Comparer<int>.Less(value);
            case "<=":
                return Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt.Comparer<int>.LessOrEquals(value);
            case ">":
                return Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt.Comparer<int>.More(value);
            case ">=":
                return Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt.Comparer<int>.MoreOrEquals(value);
            default: throw new Exception($"Unknown comparable operator '{splitter}'");
        }
    }
}
