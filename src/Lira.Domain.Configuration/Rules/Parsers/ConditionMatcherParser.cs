using Lira.Domain.Matching.Conditions;
using Lira.Domain.Matching.Conditions.Matchers;
using Lira.Domain.Matching.Conditions.Matchers.Attempt;
using Lira.Domain.Matching.Conditions.Matchers.Elapsed;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Common.PrettyParsers;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules.Parsers;
class ConditionMatcherParser
{
    private readonly IRequestStatisticStorage _requestStatisticStorage;

    public ConditionMatcherParser(IRequestStatisticStorage requestStatisticStorage)
    {
        _requestStatisticStorage = requestStatisticStorage;
    }

    static class ConditionMatcherName
    {
        public static readonly string Attempt = Consts.ControlChars.SystemVariablePrefix + "attempt";
        public static readonly string Elapsed = Consts.ControlChars.SystemVariablePrefix + "elapsed";
    }

    public ConditionMatcherSet Parse(FileSection conditionSection)
    {
        return new ConditionMatcherSet(_requestStatisticStorage, conditionSection.LinesWithoutBlock.Select(CreateConditionMatcher).ToArray());
    }

    private IConditionMatcher CreateConditionMatcher(string line)
    {
        var splitResult = line.SplitBy(">=", "<=", "=", ">", "<", "in")?.Trim();

        if (splitResult == null)
            throw new ArgumentException($"Cannot parse line '{line}'");

        var (splitter, variable, strValue) = splitResult;
        
        if (variable == ConditionMatcherName.Attempt)
        {
            if (splitter == "in")
            {
                if(!Interval<int>.TryParse(strValue, out var interval))
                    throw new ArgumentException($"Not range int value '{strValue}' in line {line}");

                return new AttemptConditionMatcher(
                    new ComparableMatchFunction<int>(ValueComparer<int>.MoreOrEquals(interval.From)),
                    new ComparableMatchFunction<int>(ValueComparer<int>.LessOrEquals(interval.To)));
            }

            if (!int.TryParse(strValue, out var value))
                throw new ArgumentException($"Not int value '{strValue}' in line {line}");

            var comparer = GetValueComparer(splitter, value);
            return new AttemptConditionMatcher(new ComparableMatchFunction<int>(comparer));
        }

        if (variable == ConditionMatcherName.Elapsed)
        {
            if (splitter == "in")
            {
                if(!Interval<TimeSpan>.TryParse(strValue, out var interval, new PrettyTimespanParser()))
                    throw new ArgumentException($"Not range int value '{strValue}' in line {line}");

                return new ElapsedConditionMatcher(
                    new ComparableMatchFunction<TimeSpan>(ValueComparer<TimeSpan>.MoreOrEquals(interval.From)),
                    new ComparableMatchFunction<TimeSpan>(ValueComparer<TimeSpan>.LessOrEquals(interval.To)));
            }
            
            var timespan = PrettyTimespanParser.Parse(strValue);
            var comparer = GetValueComparer(splitter, timespan);
            return new ElapsedConditionMatcher(new ComparableMatchFunction<TimeSpan>(comparer));

        }
        throw new Exception($"Unknown variable in condition section: '{variable}'");
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
