using Microsoft.Extensions.Configuration;

namespace SimpleMockServer.ConfigurationProviding.Rules;

internal class RulesProvider : IRulesProvider
{
    private readonly string _path;
    private readonly Task<IReadOnlyCollection<RuleWithExtInfo>> _rulesTask;
    private readonly RulesFileParser _rulesFileParser;

    public RulesProvider(IConfiguration configuration, RulesFileParser rulesFileParser)
    {
        string path = configuration.GetValue<string>(ConfigurationName.ConfigurationPath);

        _path = path;
        _rulesFileParser = rulesFileParser;
        _rulesTask = LoadRules(path);
    }

    async Task<IReadOnlyCollection<RuleWithExtInfo>> LoadRules(string path)
    {
        string[] rulesFiles = Directory.GetFiles(path, "*.rules", SearchOption.AllDirectories);
        var rules = new List<RuleWithExtInfo>(rulesFiles.Length * 3);

        foreach (string ruleFile in rulesFiles)
        {
            rules.AddRange(await _rulesFileParser.Parse(ruleFile));
        }

        return rules;
    }

    public Task<IReadOnlyCollection<RuleWithExtInfo>> GetRules()
    {
        return _rulesTask;
    }
}