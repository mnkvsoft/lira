using System.Collections.Immutable;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Configuration.Variables;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules;

internal class RuleFileParser
{
    private readonly RequestMatchersParser _requestMatchersParser;
    private readonly ConditionMatcherParser _conditionMatcherParser;
    private readonly FileSectionDeclaredItemsParser _fileSectionDeclaredItemsParser;
    private readonly HandlersParser _handlersParser;
    private readonly IFunctionFactoryCSharpFactory _functionFactoryCSharpFactory;

    public RuleFileParser(
        RequestMatchersParser requestMatchersParser,
        ConditionMatcherParser conditionMatcherParser,
        FileSectionDeclaredItemsParser fileSectionDeclaredItemsParser,
        HandlersParser handlersParser, IFunctionFactoryCSharpFactory functionFactoryCSharpFactory)
    {
        _requestMatchersParser = requestMatchersParser;
        _conditionMatcherParser = conditionMatcherParser;
        _fileSectionDeclaredItemsParser = fileSectionDeclaredItemsParser;
        _handlersParser = handlersParser;
        _functionFactoryCSharpFactory = functionFactoryCSharpFactory;
    }

    public async Task<IReadOnlyCollection<Rule>> Parse(string ruleFile, IReadonlyParsingContext parsingContext)
    {
        var sectionsRoot = await SectionFileParser.Parse(ruleFile);

        var sections = sectionsRoot.Sections;
        AssertContainsOnlySections(sections, [Constants.SectionName.Rule, Constants.SectionName.Declare, Constants.SectionName.Templates]);

        var usingContext = _functionFactoryCSharpFactory.CreateRulesUsingContext(sectionsRoot.Lines);

        // var ctx = new ParsingContext(parsingContext, cSharpUsingContext: usingContext, currentPath: ruleFile.GetDirectory());
        var ctx = new ParsingContext(parsingContext, cSharpUsingContext: usingContext, currentPath: ruleFile.GetDirectory());

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
        ParsingContext parsingContext)
    {
        var childSections = ruleSection.ChildSections;

        if (childSections.Count == 0)
            throw new Exception("Rule section is empty");

        var ctx = new ParsingContext(parsingContext);
        var requestMatchers = await _requestMatchersParser.Parse(ruleSection, ctx);

        var declaredItems = await GetDeclaredItems(childSections, ctx);
        ctx.SetDeclaredItems(declaredItems);

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
                    rulesSections: childConditionSections,
                    expectedSectionName: _handlersParser.GetAllSectionNames(childConditionSections).NewWith(Constants.SectionName.Response, Constants.SectionName.Declare));

                ctx.SetDeclaredItems(await GetDeclaredItems(childConditionSections, ctx));

                var handlers = await _handlersParser.Parse(childConditionSections, ctx);

                var conditionMatchers = _conditionMatcherParser.Parse(conditionSection);

                var matchers = new List<IRequestMatcher>();
                matchers.AddRange(requestMatchers);
                matchers.AddRange(conditionMatchers);

                rules.Add(
                    new Rule(
                        ruleName + $". Condition no. {i + 1}",
                        matchers,
                        handlers)
                );
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
            _handlersParser.GetAllSectionNames(childSections).NewWith(Constants.SectionName.Declare, Constants.SectionName.Templates));

        var handlerss = await _handlersParser.Parse(childSections, ctx);
        return
        [
            new Rule(
            ruleName,
            requestMatchers,
            handlerss)
        ];
    }

    private async Task<DeclaredItems> GetDeclaredItems(IReadOnlyCollection<FileSection> childSections, IReadonlyParsingContext parsingContext)
    {
        var result = DeclaredItems.WithoutLocalVariables(parsingContext.DeclaredItems);

        var variablesSection = childSections.FirstOrDefault(x => x.Name == Constants.SectionName.Declare);
        if (variablesSection != null)
        {
            result.TryAddRange(await _fileSectionDeclaredItemsParser.Parse(variablesSection, parsingContext));
        }

        return result;
    }

    private static void AssertContainsOnlySections(IImmutableList<FileSection> rulesSections,
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
