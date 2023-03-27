namespace SimpleMockServer.ConfigurationProviding.Rules;

public interface IRulesProvider
{
    Task<IReadOnlyCollection<RuleWithExtInfo>> GetRules();
}