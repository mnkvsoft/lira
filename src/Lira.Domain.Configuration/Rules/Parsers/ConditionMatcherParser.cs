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
        public static readonly string Attempt = "attempt";
        public static readonly string Elapsed = "elapsed";
    }

    public IReadOnlyCollection<IRequestMatcher> Parse(FileSection conditionSection)
    {
        return conditionSection.LinesWithoutBlock.Select(CreateConditionMatcher).ToArray();
    }

    private IRequestMatcher CreateConditionMatcher(string line)
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
                    _requestStatisticStorage,
                    new ComparableMatchFunction<int>(ValueComparer<int>.MoreOrEquals(interval.From)),
                    new ComparableMatchFunction<int>(ValueComparer<int>.LessOrEquals(interval.To)));
            }

            if (!int.TryParse(strValue, out var value))
                throw new ArgumentException($"Not int value '{strValue}' in line {line}");

            var comparer = GetValueComparer(splitter, value);
            return new AttemptConditionMatcher(_requestStatisticStorage, new ComparableMatchFunction<int>(comparer));
        }

        if (variable == ConditionMatcherName.Elapsed)
        {
            if (splitter == "in")
            {
                if(!Interval<TimeSpan>.TryParse(strValue, out var interval, new PrettyTimespanParser()))
                    throw new ArgumentException($"Not range int value '{strValue}' in line {line}");

                return new ElapsedConditionMatcher(
                    _requestStatisticStorage,
                    new ComparableMatchFunction<TimeSpan>(ValueComparer<TimeSpan>.MoreOrEquals(interval.From)),
                    new ComparableMatchFunction<TimeSpan>(ValueComparer<TimeSpan>.LessOrEquals(interval.To)));
            }
            
            var timespan = PrettyTimespanParser.Parse(strValue);
            var comparer = GetValueComparer(splitter, timespan);
            return new ElapsedConditionMatcher(_requestStatisticStorage, new ComparableMatchFunction<TimeSpan>(comparer));

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
