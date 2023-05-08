using Microsoft.Extensions.Logging;
using SimpleMockServer.Domain.Configuration.Rules.Parsers.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules;

internal class RulesProvider
{
    private readonly RulesFileParser _rulesFileParser;
    private readonly GlobalVariablesParser _globalVariablesParser;
    private readonly ILogger _logger;

    public RulesProvider(ILoggerFactory loggerFactory, RulesFileParser rulesFileParser, GlobalVariablesParser globalVariablesParser)
    {
        _globalVariablesParser = globalVariablesParser;
        _rulesFileParser = rulesFileParser;
        _logger = loggerFactory.CreateLogger(GetType());
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

        _logger.LogInformation($"{rules.Count} rules was loaded");
        
        return rules;
    }
}
