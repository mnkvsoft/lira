using Microsoft.Extensions.Logging;
using Lira.Common.Extensions;
using Lira.Domain.Actions;
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
    private readonly ResponseStrategyParser _responseStrategyParser;
    private readonly ConditionMatcherParser _conditionMatcherParser;
    private readonly FileSectionDeclaredItemsParser _fileSectionDeclaredItemsParser;
    private readonly ActionsParser _actionsParser;

    public RuleFileParser(
        ILoggerFactory loggerFactory,
        RequestMatchersParser requestMatchersParser,
        ResponseStrategyParser responseStrategyParser,
        ConditionMatcherParser conditionMatcherParser,
        FileSectionDeclaredItemsParser fileSectionDeclaredItemsParser,
        ActionsParser actionsParser)
    {
        _loggerFactory = loggerFactory;
        _requestMatchersParser = requestMatchersParser;
        _responseStrategyParser = responseStrategyParser;
        _conditionMatcherParser = conditionMatcherParser;
        _fileSectionDeclaredItemsParser = fileSectionDeclaredItemsParser;
        _actionsParser = actionsParser;
    }

    public async Task<IReadOnlyCollection<Rule>> Parse(string ruleFile, IReadonlyParsingContext parsingContext)
    {
        var sections = await SectionFileParser.Parse(ruleFile);

        AssertContainsOnlySections(sections, [Constants.SectionName.Rule, Constants.SectionName.Declare, Constants.SectionName.Templates]);

        var ctx = new ParsingContext(parsingContext, currentPath: ruleFile.GetDirectory());

        var templates = GetTemplates(sections, ctx);
        ctx.SetTemplates(templates);

        var declaredItems = await GetDeclaredItems(sections, ctx);
        ctx.SetDeclaredItems(declaredItems);

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
                ctx));
        }

        return rules;
    }

    private async Task<IReadOnlyCollection<Rule>> CreateRules(
        string ruleName,
        FileSection ruleSection,
        IReadonlyParsingContext parsingContext)
    {
        var childSections = ruleSection.ChildSections;

        if (childSections.Count == 0)
            throw new Exception("Rule section is empty");

        var ctx = new ParsingContext(parsingContext);
        var requestMatchers = _requestMatchersParser.Parse(ruleSection, ctx);

        var templates = GetTemplates(childSections, parsingContext);
        ctx.SetTemplates(templates);

        var declaredItems = await GetDeclaredItems(childSections, ctx);
        ctx.SetDeclaredItems(declaredItems);

        ResponseStrategy responseStrategy;
        IReadOnlyCollection<Delayed<IAction>> externalCallers;

        bool existsConditionSection = childSections.Any(x => x.Name == Constants.SectionName.Condition);
        bool existsCacheSection = childSections.Any(x => x.Name == Constants.SectionName.Cache);

        if (existsConditionSection && existsCacheSection)
            throw new Exception("Section  and 2 cannot be declared together");

        if (existsConditionSection)
        {
            AssertContainsOnlySections(childSections, [Constants.SectionName.Condition, Constants.SectionName.Declare, Constants.SectionName.Templates]);

            var conditionSections = childSections.Where(s => s.Name == Constants.SectionName.Condition).ToArray();

            if (conditionSections.Length < 2)
                throw new Exception($"Must be at least 2 '{Constants.SectionName.Condition}' sections");

            var rules = new List<Rule>();

            for (var i = 0; i < conditionSections.Length; i++)
            {
                var conditionSection = conditionSections[i];
                var childConditionSections = conditionSection.ChildSections;
                AssertContainsOnlySections(
                    childConditionSections, _actionsParser.GetSectionNames(childConditionSections).NewWith(Constants.SectionName.Response, Constants.SectionName.Declare));

                ctx.SetDeclaredItems(await GetDeclaredItems(childConditionSections, ctx));

                responseStrategy = await _responseStrategyParser.Parse(conditionSection, ctx);
                externalCallers = await _actionsParser.Parse(childConditionSections, ctx);

                var conditionMatchers = _conditionMatcherParser.Parse(conditionSection);

                var matchers = new List<IRequestMatcher>();
                matchers.AddRange(requestMatchers);
                matchers.AddRange(conditionMatchers);

                rules.Add(new Rule(
                    ruleName + $". Condition no. {i + 1}",
                    new RequestMatcherSet(matchers),
                    new ActionsExecutor(externalCallers, _loggerFactory),
                    responseStrategy));
            }

            return rules;
        }

        //if (existsCacheSection)
        //{
        //    AssertContainsOnlySections(childSections, new[] { Constants.SectionName.Cache, Constants.SectionName.Declare, Constants.SectionName.Templates });

        //    var conditionSections = childSections.Where(s => s.Name == Constants.SectionName.Cache).ToArray();

        //    if (conditionSections.Length > 1)
        //        throw new Exception($"There can only be one {Constants.SectionName.Cache} section");

        //    var cacheSection = childSections.Single(s => s.Name == Constants.SectionName.Cache);

        //    var rules = new List<Rule>();

        //    if(cacheSection.ChildSections.Count > 0)
        //        throw new Exception($"Section {Constants.SectionName.Cache} cannot contains child section. Contains: {string.Join(", ", cacheSection.ChildSections.Select(x => x.Name))}");



        //    for (var i = 0; i < conditionSections.Length; i++)
        //    {
        //        var conditionSection = conditionSections[i];
        //        var childConditionSections = conditionSection.ChildSections;
        //        AssertContainsOnlySections(
        //            childConditionSections, _actionsParser.GetSectionNames(childConditionSections).NewWith(Constants.SectionName.Response, Constants.SectionName.Declare));

        //        ctx = ctx with { DeclaredItems = await GetDeclaredItems(childConditionSections, ctx) };

        //        responseStrategy = await _responseStrategyParser.Parse(conditionSection, ctx);
        //        externalCallers = await _actionsParser.Parse(childConditionSections, ctx);

        //        var conditionMatchers = _conditionMatcherParser.Parse(conditionSection);

        //        var matchers = new List<IRequestMatcher>();
        //        matchers.AddRange(requestMatchers);
        //        matchers.AddRange(conditionMatchers);

        //        rules.Add(new Rule(
        //            ruleName + $". Condition no. {i + 1}",
        //            new RequestMatcherSet(matchers),
        //            new ActionsExecutor(externalCallers, _loggerFactory),
        //            responseStrategy));
        //    }

        //    return rules;
        //}

        AssertContainsOnlySections(
            childSections,
            _actionsParser.GetSectionNames(childSections).NewWith(Constants.SectionName.Response, Constants.SectionName.Declare, Constants.SectionName.Templates));

        responseStrategy = await _responseStrategyParser.Parse(ruleSection, ctx);
        externalCallers = await _actionsParser.Parse(childSections, ctx);

        return new[] { new Rule(
            ruleName,
            new RequestMatcherSet(requestMatchers),
            new ActionsExecutor(externalCallers, _loggerFactory),
            responseStrategy)};
    }

    private TemplateSet GetTemplates(IReadOnlyCollection<FileSection> childSections, IReadonlyParsingContext parsingContext)
    {
        var result = new TemplateSet(parsingContext.Templates);

        var templatesSection = childSections.FirstOrDefault(x => x.Name == Constants.SectionName.Templates);
        if (templatesSection != null)
            result.AddRange(TemplatesParser.Parse(templatesSection.LinesWithoutBlock));

        return result;
    }

    private async Task<DeclaredItems> GetDeclaredItems(IReadOnlyCollection<FileSection> childSections, IReadonlyParsingContext parsingContext)
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
