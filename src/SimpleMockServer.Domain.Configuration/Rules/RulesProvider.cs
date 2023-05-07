using SimpleMockServer.Domain.Configuration.Rules.Parsers.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules;

internal class RulesProvider
{
    private readonly RulesFileParser _rulesFileParser;
    private readonly GlobalVariablesParser _globalVariablesParser;

    public RulesProvider(RulesFileParser rulesFileParser, GlobalVariablesParser globalVariablesParser)
    {
        _globalVariablesParser = globalVariablesParser;
        _rulesFileParser = rulesFileParser;
    }
    
    public async Task<IReadOnlyCollection<Rule>> LoadRules(string path)
    {
        await _globalVariablesParser.Load(path);

        var rulesFiles = Directory.GetFiles(path, "*.rules", SearchOption.AllDirectories);
        var rules = new List<Rule>(rulesFiles.Length * 3);

        foreach (var ruleFile in rulesFiles)
        {
            try
            {
                rules.AddRange(await _rulesFileParser.Parse(ruleFile));
            }
            catch (Exception exc)
            {
                throw new FileParsingException(ruleFile, exc);
            }
        }

        return rules;
    }
}
