using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Elapsed;
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
        public const string Attempt = Constants.ControlChars.VariablePrefix + "attempt";
        public const string Elapsed = Constants.ControlChars.VariablePrefix + "elapsed";
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

        if (splitResult.LeftPart == ConditionMatcherName.Attempt)
        {
            if (!int.TryParse(splitResult.RightPart, out int value))
                throw new ArgumentException($"Not int value '{splitResult.RightPart}' in line {line}");

            var comparer = GetValueComparer(splitResult.Splitter, value);
            return new AttemptConditionMatcher(new ComparableMatchFunction<int>(comparer));
        }
        else if (splitResult.LeftPart == ConditionMatcherName.Elapsed)
        {
            var timespan = PrettyTimespanParser.Parse(splitResult.RightPart);
            var comparer = GetValueComparer(splitResult.Splitter, timespan);
            return new ElapsedConditionMatcher(new ComparableMatchFunction<TimeSpan>(comparer));

        }
        throw new Exception($"Unknown variable in condition section: '{splitResult.LeftPart}'");
    }

    private ValueComparer<T> GetValueComparer<T>(string splitter, T value) where T : IComparable<T>
    {
        switch (splitter)
        {
            case "=":
                return ValueComparer<T>.AreEquals(value);
            case "<":
                return ValueComparer<T>.Less(value);
            case "<=":
                return ValueComparer<T>.LessOrEquals(value);
            case ">":
                return ValueComparer<T>.More(value);
            case ">=":
                return ValueComparer<T>.MoreOrEquals(value);
            default: throw new Exception($"Unknown comparable operator '{splitter}'");
        }
    }
}
