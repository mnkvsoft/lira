namespace Lira.Domain.Configuration.Rules;

public interface IRulesProvider
{
    Task<IReadOnlyCollection<Rule>> GetRules();
}
