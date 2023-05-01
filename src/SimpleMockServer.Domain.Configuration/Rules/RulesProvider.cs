using Microsoft.Extensions.Configuration;

namespace SimpleMockServer.Domain.Configuration.Rules;

internal class RulesProvider : IRulesProvider
{
    private readonly Task<IReadOnlyCollection<Rule>> _rulesTask;
    private readonly RulesFileParser _rulesFileParser;

    public RulesProvider(IConfiguration configuration, RulesFileParser rulesFileParser)
    {
        var path = configuration.GetValue<string>(ConfigurationName.ConfigurationPath);

        _rulesFileParser = rulesFileParser;
        _rulesTask = LoadRules(path);
    }

    async Task<IReadOnlyCollection<Rule>> LoadRules(string path)
    {
        var rulesFiles = Directory.GetFiles(path, "*.rules", SearchOption.AllDirectories);
        var rules = new List<Rule>(rulesFiles.Length * 3);

        foreach (var ruleFile in rulesFiles)
        {
            rules.AddRange(await _rulesFileParser.Parse(ruleFile));
        }

        return rules;
    }

    public Task<IReadOnlyCollection<Rule>> GetRules()
    {
        return _rulesTask;
    }
}
