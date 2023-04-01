

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;

public interface IIntMatchFunction
{
    bool IsMatch(int value);
}
