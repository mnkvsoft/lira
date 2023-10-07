using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;

namespace Lira.Domain.Configuration.Rules;

internal class RulesLoader
{
    private readonly RuleFileParser _ruleFileParser;
    
    private readonly ILogger _logger;

    public RulesLoader(ILoggerFactory loggerFactory, RuleFileParser ruleFileParser)
    {
        _ruleFileParser = ruleFileParser;
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    public async Task<IReadOnlyCollection<Rule>> LoadRules(string path, ParsingContext parsingContext)
    {
        var sw = Stopwatch.StartNew();

        var rulesFiles = DirectoryHelper.GetFiles(path, "*.rules");
        var rules = new List<Rule>(rulesFiles.Count * 3);

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

        _logger.LogInformation($"{rules.Count} rules was loaded ({(int)sw.ElapsedMilliseconds} ms)");
        
        return rules;
    }
}
