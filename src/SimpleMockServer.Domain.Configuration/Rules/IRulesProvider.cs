namespace SimpleMockServer.Domain.Configuration.Rules;

public interface IRulesProvider
{
    Task<IReadOnlyCollection<Rule>> GetRules();
}

internal class RulesProviderWithState : StatedProvider<IReadOnlyCollection<Rule>>, IRulesProvider
{
    public RulesProviderWithState(IConfigurationPathProvider configuration, RulesProvider rulesProvider) : base(configuration, rulesProvider.LoadRules)
    {
    }

    public Task<IReadOnlyCollection<Rule>> GetRules()
    {
        return LoadTask;
    }
}
