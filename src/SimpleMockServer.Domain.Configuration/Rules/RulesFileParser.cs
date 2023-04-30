using Microsoft.Extensions.Logging;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.Parsers;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules;


internal class RulesFileParser
{
    private readonly ILoggerFactory _loggerFactory;
    
    private readonly RequestMatchersParser _requestMatchersParser;
    private readonly ResponseWriterParser _responseWriterParser;
    private readonly ConditionMatcherParser _conditionMatcherParser;
    private readonly VariablesParser _variablesParser;
    private readonly ExternalCallerParser _externalCallerParser;

    public RulesFileParser(
        ILoggerFactory loggerFactory,
        RequestMatchersParser requestMatchersParser,
        ResponseWriterParser responseWriterParser,
        ConditionMatcherParser conditionMatcherParser,
        VariablesParser variablesParser,
        ExternalCallerParser externalCallerParser)
    {
        _loggerFactory = loggerFactory;
        _requestMatchersParser = requestMatchersParser;
        _responseWriterParser = responseWriterParser;
        _conditionMatcherParser = conditionMatcherParser;
        _variablesParser = variablesParser; ;
        _externalCallerParser = externalCallerParser;
    }

    public async Task<IReadOnlyCollection<Rule>> Parse(string ruleFile)
    {
        try
        {
            var knownSectionsBlocks = _externalCallerParser.GetSectionsKnowsBlocks();

            var externalCallerSections = knownSectionsBlocks.Keys.ToList();

            knownSectionsBlocks.Add("rule", BlockNameHelper.GetBlockNames<Constants.BlockName.Rule>());
            knownSectionsBlocks.Add("response", BlockNameHelper.GetBlockNames<Constants.BlockName.Response>());

            var rulesSections = await SectionFileParser.Parse(
                ruleFile,
                knownBlockForSections: knownSectionsBlocks,
                maxNestingDepth: 3);

            AssertContainsOnlySections(rulesSections, new [] { Constants.SectionName.Rule });

            var rules = new List<Rule>();
            for (int i = 0; i < rulesSections.Count; i++)
            {
                var fi = new FileInfo(ruleFile);
                var ruleName = $"no. {i + 1} file: {fi.Name}";

                var ruleSection = rulesSections[i];
                rules.AddRange(CreateRules(ruleName, ruleSection, externalCallerSections));
            }

            return rules;
        }
        catch (Exception exc)
        {
            throw new Exception("An error occured while parsing file: " + ruleFile, exc);
        }
    }

    private IReadOnlyCollection<Rule> CreateRules(string ruleName, FileSection ruleSection, IReadOnlyCollection<string> externalCallerSections)
    {
        var childSections = ruleSection.ChildSections;

        if (childSections.Count == 0)
            throw new Exception("Rule section is empty");

        var requestMatcherSet = _requestMatchersParser.Parse(ruleSection);

        var variablesSection = childSections.FirstOrDefault(x => x.Name == Constants.SectionName.Variables);
        var variables = variablesSection == null ? new VariableSet() : _variablesParser.Parse(variablesSection);

        var exitstConditionSection = childSections.Any(x => x.Name == Constants.SectionName.Condition);

        Delayed<ResponseWriter> responseWriter;
        IReadOnlyCollection<Delayed<IExternalCaller>> externalCallers;

        if (exitstConditionSection)
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

                responseWriter = _responseWriterParser.Parse(conditionSection, variables);
                externalCallers = _externalCallerParser.Parse(childConditionSections, variables);

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

        responseWriter = _responseWriterParser.Parse(ruleSection, variables);
        externalCallers = _externalCallerParser.Parse(childSections, variables);

        return new[] { new Rule(ruleName, _loggerFactory, responseWriter, requestMatcherSet, conditionMatcherSet: null, externalCallers) };
    }

    private static void AssertContainsOnlySections(IReadOnlyList<FileSection> rulesSections, IReadOnlyCollection<string> expectedSectionName)
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
