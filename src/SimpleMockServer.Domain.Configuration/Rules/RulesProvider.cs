using Microsoft.Extensions.Configuration;
using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.ConfigurationProviding.Rules;

internal class RulesProvider : IRulesProvider
{
    private readonly Task<IReadOnlyCollection<Rule>> _rulesTask;
    private readonly RulesFileParser _rulesFileParser;

    public RulesProvider(IConfiguration configuration, RulesFileParser rulesFileParser)
    {
        string path = configuration.GetValue<string>(ConfigurationName.ConfigurationPath);

        _rulesFileParser = rulesFileParser;
        _rulesTask = LoadRules(path);
    }

    async Task<IReadOnlyCollection<Rule>> LoadRules(string path)
    {
        string[] rulesFiles = Directory.GetFiles(path, "*.rules", SearchOption.AllDirectories);
        var rules = new List<Rule>(rulesFiles.Length * 3);

        foreach (string ruleFile in rulesFiles)
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
