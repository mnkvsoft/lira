using Lira.Domain.Configuration.Rules.ValuePatternParsing;

namespace Lira.Domain.Configuration.Rules;

internal class RulesLoader
{
    private readonly RuleFileParser _ruleFileParser;

    public RulesLoader(RuleFileParser ruleFileParser) => _ruleFileParser = ruleFileParser;

    public async Task<IReadOnlyCollection<RuleData>> LoadRulesDatas(string path, IReadonlyParsingContext parsingContext)
    {
        var rulesFiles = DirectoryHelper.GetFiles(path, "*.rules");
        var rules = new List<RuleData>(rulesFiles.Count * 3);

        foreach (var ruleFile in rulesFiles)
        {
            try
            {
                rules.AddRange(await _ruleFileParser.Parse(ruleFile, parsingContext));
            }
            catch (Exception exc)
            {
                throw new FileParsingException(ruleFile, exc);
            }
        }

        return rules;
    }
}
