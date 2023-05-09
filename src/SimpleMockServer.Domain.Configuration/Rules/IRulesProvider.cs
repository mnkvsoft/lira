namespace SimpleMockServer.Domain.Configuration.Rules;

public interface IRulesProvider
{
    Task<IReadOnlyCollection<Rule>> GetRules();
}
