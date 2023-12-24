using Microsoft.Extensions.Logging;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Configuration.Templating;
using Lira.Domain.Configuration.Variables;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules;

internal class RuleFileParser
{
    private readonly ILoggerFactory _loggerFactory;

    private readonly RequestMatchersParser _requestMatchersParser;
    private readonly ResponseWriterParser _responseWriterParser;
    private readonly ConditionMatcherParser _conditionMatcherParser;
    private readonly FileSectionDeclaredItemsParser _fileSectionDeclaredItemsParser;
    private readonly ExternalCallerParser _externalCallerParser;

    public RuleFileParser(
        ILoggerFactory loggerFactory,
        RequestMatchersParser requestMatchersParser,
        ResponseWriterParser responseWriterParser,
        ConditionMatcherParser conditionMatcherParser,
        FileSectionDeclaredItemsParser fileSectionDeclaredItemsParser,
        ExternalCallerParser externalCallerParser)
    {
        _loggerFactory = loggerFactory;
        _requestMatchersParser = requestMatchersParser;
        _responseWriterParser = responseWriterParser;
        _conditionMatcherParser = conditionMatcherParser;
        _fileSectionDeclaredItemsParser = fileSectionDeclaredItemsParser;
        _externalCallerParser = externalCallerParser;
    }

    public async Task<IReadOnlyCollection<Rule>> Parse(string ruleFile, ParsingContext parsingContext)
    {
        var externalCallerSections = _externalCallerParser.GetSectionNames();

        var sections = await SectionFileParser.Parse(ruleFile);
        
        AssertContainsOnlySections(sections, new[] { Constants.SectionName.Rule, Constants.SectionName.Declare, Constants.SectionName.Templates });
        
        var ctx = parsingContext with { CurrentPath = ruleFile.GetDirectory() };
        
        var templates = GetTemplates(sections, parsingContext);
        ctx = ctx with { Templates = templates };
        
        var variables = await GetDeclaredItems(sections, parsingContext);
        ctx = ctx with { DeclaredItems = variables };
        
        var rules = new List<Rule>();
        var ruleSections = sections.Where(s => s.Name == Constants.SectionName.Rule).ToArray();
        for (var i = 0; i < ruleSections.Length; i++)
        {
            var fi = new FileInfo(ruleFile);
            var ruleName = $"no. {i + 1} file: {fi.FullName}";

            var ruleSection = ruleSections[i];
            
            rules.AddRange(await CreateRules(
                ruleName, 
                ruleSection, 
                externalCallerSections, 
                ctx));
        }

        return rules;
    }

    private async Task<IReadOnlyCollection<Rule>> CreateRules(string ruleName, FileSection ruleSection,
        IReadOnlyCollection<string> externalCallerSections, ParsingContext parsingContext)
    {
        var childSections = ruleSection.ChildSections;

        if (childSections.Count == 0)
            throw new Exception("Rule section is empty");

        var (requestMatcherSet, pathNameMaps) = _requestMatchersParser.Parse(ruleSection, parsingContext);

        var templates = GetTemplates(childSections, parsingContext);
        var ctx = parsingContext with { Templates = templates };
        
        var variables = await GetDeclaredItems(childSections, ctx);
        ctx = ctx with { DeclaredItems = variables };
        
        var existConditionSection = childSections.Any(x => x.Name == Constants.SectionName.Condition);

        Delayed<ResponseWriter> responseWriter;
        IReadOnlyCollection<Delayed<IExternalCaller>> externalCallers;

        if (existConditionSection)
        {
            AssertContainsOnlySections(childSections, new[] { Constants.SectionName.Condition, Constants.SectionName.Declare, Constants.SectionName.Declare });

            if (childSections.Count < 2)
                throw new Exception($"Must be at least 2 '{Constants.SectionName.Condition}' sections");

            var rules = new List<Rule>();
            for (var i = 0; i < childSections.Count; i++)
            {
                var conditionSection = childSections[i];
                var childConditionSections = conditionSection.ChildSections;
                AssertContainsOnlySections(childConditionSections, externalCallerSections.NewWith(Constants.SectionName.Response));

                var conditionMatcherSet = _conditionMatcherParser.Parse(conditionSection);

                responseWriter = await _responseWriterParser.Parse(conditionSection, ctx);
                externalCallers = await _externalCallerParser.Parse(childConditionSections, ctx);

                rules.Add(new Rule(
                    ruleName + $". Condition no. {i + 1}",
                    _loggerFactory,
                    responseWriter,
                    requestMatcherSet,
                    pathNameMaps,
                    conditionMatcherSet,
                    externalCallers));
            }

            return rules;
        }

        AssertContainsOnlySections(
            childSections,
            externalCallerSections.NewWith(Constants.SectionName.Response, Constants.SectionName.Declare, Constants.SectionName.Templates));

        responseWriter = await _responseWriterParser.Parse(ruleSection, ctx);
        externalCallers = await _externalCallerParser.Parse(childSections, ctx);

        return new[] { new Rule(ruleName, _loggerFactory, responseWriter, requestMatcherSet, pathNameMaps, conditionMatcherSet: null, externalCallers) };
    }

    private IReadOnlyCollection<Template> GetTemplates(IReadOnlyCollection<FileSection> childSections, ParsingContext parsingContext)
    {
        var result = new TemplateSet(parsingContext.Templates);
        
        var templatesSection = childSections.FirstOrDefault(x => x.Name == Constants.SectionName.Templates);
        if (templatesSection != null)
            result.AddRange(TemplatesParser.Parse(templatesSection.LinesWithoutBlock));
        
        return result;
    }
    
    private async Task<IReadonlyDeclaredItems> GetDeclaredItems(IReadOnlyCollection<FileSection> childSections, ParsingContext parsingContext)
    {
        var result = new DeclaredItems(parsingContext.DeclaredItems);
        
        var variablesSection = childSections.FirstOrDefault(x => x.Name == Constants.SectionName.Declare);
        if (variablesSection != null)
        {
            result.Add(await _fileSectionDeclaredItemsParser.Parse(variablesSection, parsingContext));
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
