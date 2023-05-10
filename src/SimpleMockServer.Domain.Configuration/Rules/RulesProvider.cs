﻿using Microsoft.Extensions.Logging;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules;

internal class RulesLoader
{
    private readonly RuleFileParser _ruleFileParser;
    
    private readonly ILogger _logger;

    public RulesLoader(ILoggerFactory loggerFactory, RuleFileParser ruleFileParser)
    {
        _ruleFileParser = ruleFileParser;
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    public async Task<IReadOnlyCollection<Rule>> LoadRules(string path, IReadOnlyCollection<Variable> registeredVariables)
    {
        var rulesFiles = Directory.GetFiles(path, "*.rules", SearchOption.AllDirectories);
        var rules = new List<Rule>(rulesFiles.Length * 3);

        foreach (var ruleFile in rulesFiles)
        {
            try
            {
                rules.AddRange(await _ruleFileParser.Parse(ruleFile, registeredVariables));
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
