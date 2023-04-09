using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.ConfigurationProviding.Rules;

public interface IRulesProvider
{
    Task<IReadOnlyCollection<Rule>> GetRules();
}
