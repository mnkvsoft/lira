using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;

namespace SimpleMockServer.Domain.Models.RulesModel;

public interface IConditionMatcher
{
    bool IsMatch(RequestStatistic statistic);
}
