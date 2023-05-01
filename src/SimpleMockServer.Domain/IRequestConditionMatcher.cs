using SimpleMockServer.Domain.Matching.Conditions;

namespace SimpleMockServer.Domain;

public interface IConditionMatcher
{
    bool IsMatch(RequestStatistic statistic);
}
