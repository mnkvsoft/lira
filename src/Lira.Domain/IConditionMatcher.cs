using Lira.Domain.Matching.Conditions;

namespace Lira.Domain;

public interface IConditionMatcher
{
    bool IsMatch(RequestStatistic statistic);
}
