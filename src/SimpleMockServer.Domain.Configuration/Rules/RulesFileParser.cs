using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.Parsers;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Configuration.Variables;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules;

internal class RuleFileParser
{
    private readonly ILoggerFactory _loggerFactory;

    private readonly RequestMatchersParser _requestMatchersParser;
    private readonly ResponseWriterParser _responseWriterParser;
    private readonly ConditionMatcherParser _conditionMatcherParser;
    private readonly FileSectionVariablesParser _fileSectionVariablesParser;
    private readonly ExternalCallerParser _externalCallerParser;

    public RuleFileParser(
        ILoggerFactory loggerFactory,
        RequestMatchersParser requestMatchersParser,
        ResponseWriterParser responseWriterParser,
        ConditionMatcherParser conditionMatcherParser,
        FileSectionVariablesParser fileSectionVariablesParser,
        ExternalCallerParser externalCallerParser)
    {
        _loggerFactory = loggerFactory;
        _requestMatchersParser = requestMatchersParser;
        _responseWriterParser = responseWriterParser;
        _conditionMatcherParser = conditionMatcherParser;
        _fileSectionVariablesParser = fileSectionVariablesParser;
        _externalCallerParser = externalCallerParser;
    }

    public async Task<IReadOnlyCollection<Rule>> Parse(string ruleFile, ParsingContext parsingContext)
    {
        var knownSectionsBlocks = _externalCallerParser.GetSectionsKnowsBlocks();

        var externalCallerSections = knownSectionsBlocks.Keys.ToList();

        knownSectionsBlocks.Add("rule", BlockNameHelper.GetBlockNames<Constants.BlockName.Rule>());
        knownSectionsBlocks.Add("response", BlockNameHelper.GetBlockNames<Constants.BlockName.Response>());

        var sections = await SectionFileParser.Parse(
            ruleFile,
            knownBlockForSections: knownSectionsBlocks,
            maxNestingDepth: 3);

        AssertContainsOnlySections(sections, new[] { Constants.SectionName.Rule, Constants.SectionName.Variables });
        var variables = await GetVariables(sections, parsingContext);
        var context = parsingContext with { Variables = variables, CurrentPath = ruleFile.GetDirectory() };
        
        var rules = new List<Rule>();
        var ruleSections = sections.Where(s => s.Name == Constants.SectionName.Rule).ToArray();
        for (var i = 0; i < ruleSections.Length; i++)
        {
            var fi = new FileInfo(ruleFile);
            var ruleName = $"no. {i + 1} file: {fi.Name}";

            var ruleSection = ruleSections[i];
            
            rules.AddRange(await CreateRules(
                ruleName, 
                ruleSection, 
                externalCallerSections, 
                context));
        }

        return rules;
    }

    private async Task<IReadOnlyCollection<Rule>> CreateRules(string ruleName, FileSection ruleSection,
        IReadOnlyCollection<string> externalCallerSections, ParsingContext parsingContext)
    {
        var childSections = ruleSection.ChildSections;

        if (childSections.Count == 0)
            throw new Exception("Rule section is empty");

        var requestMatcherSet = _requestMatchersParser.Parse(ruleSection);

        var variables = await GetVariables(childSections, parsingContext);
        var fileRulesContext = parsingContext with { Variables = variables };
        
        var existConditionSection = childSections.Any(x => x.Name == Constants.SectionName.Condition);

        Delayed<ResponseWriter> responseWriter;
        IReadOnlyCollection<Delayed<IExternalCaller>> externalCallers;

        if (existConditionSection)
        {
            AssertContainsOnlySections(childSections, new[] { Constants.SectionName.Condition, Constants.SectionName.Variables });

            if (childSections.Count < 2)
                throw new Exception($"Must be at least 2 '{Constants.SectionName.Condition}' sections");

            var rules = new List<Rule>();
            for (var i = 0; i < childSections.Count; i++)
            {
                var conditionSection = childSections[i];
                var childConditionSections = conditionSection.ChildSections;
                AssertContainsOnlySections(childConditionSections, externalCallerSections.NewWith(Constants.SectionName.Response));

                var conditionMatcherSet = _conditionMatcherParser.Parse(conditionSection);

                responseWriter = await _responseWriterParser.Parse(conditionSection, fileRulesContext);
                externalCallers = await _externalCallerParser.Parse(childConditionSections, fileRulesContext);

                rules.Add(new Rule(
                    ruleName + $". Condition no. {i + 1}",
                    _loggerFactory,
                    responseWriter,
                    requestMatcherSet,
                    conditionMatcherSet,
                    externalCallers));
            }

            return rules;
        }

        AssertContainsOnlySections(
            childSections,
            externalCallerSections.NewWith(Constants.SectionName.Response, Constants.SectionName.Variables));

        responseWriter = await _responseWriterParser.Parse(ruleSection, fileRulesContext);
        externalCallers = await _externalCallerParser.Parse(childSections, fileRulesContext);

        return new[] { new Rule(ruleName, _loggerFactory, responseWriter, requestMatcherSet, conditionMatcherSet: null, externalCallers) };
    }

    private async Task<IReadOnlyCollection<Variable>> GetVariables(IReadOnlyCollection<FileSection> childSections, ParsingContext parsingContext)
    {
        var result = new VariableSet(parsingContext.Variables);
        
        var variablesSection = childSections.FirstOrDefault(x => x.Name == Constants.SectionName.Variables);
        if (variablesSection != null)
        {
            result.AddRange(await _fileSectionVariablesParser.Parse(variablesSection, parsingContext));
        }
        
        return result;
    }

    private static void AssertContainsOnlySections(IReadOnlyList<FileSection> rulesSections,
        IReadOnlyCollection<string> expectedSectionName)
    {
        var unknownSections = rulesSections
            .Where(s => !expectedSectionName.Contains(s.Name))
            .Select(x => x.Name)
            .ToArray();

        if (unknownSections.Length != 0)
        {
            throw new Exception(
                "Unknown sections: " + string.Join(", ", unknownSections) + ". " +
                "Expected: " + string.Join(", ", expectedSectionName));
        }
    }
}
