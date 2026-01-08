using System.Collections.Immutable;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Common.PrettyParsers;
using Lira.Domain.Caching;
using Lira.Domain.Configuration.DeclarationItems;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling.Generating;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.FileSectionFormat;
using Microsoft.Extensions.Configuration;

namespace Lira.Domain.Configuration.Rules;

internal class RuleFileLoader
{
    private readonly RequestMatchersParser _requestMatchersParser;
    private readonly ConditionMatcherParser _conditionMatcherParser;
    private readonly FileSectionDeclaredItemsParser _fileSectionDeclaredItemsParser;
    private readonly MiddlewaresParser _middlewaresParser;
    private readonly RuleKeyExtractorParser _keyExtractorParser;

    private readonly IFunctionFactoryCSharpFactory _functionFactoryCSharpFactory;
    private readonly IConfiguration _configuration;

    public RuleFileLoader(
        RequestMatchersParser requestMatchersParser,
        ConditionMatcherParser conditionMatcherParser,
        FileSectionDeclaredItemsParser fileSectionDeclaredItemsParser,
        MiddlewaresParser middlewaresParser,
        IFunctionFactoryCSharpFactory functionFactoryCSharpFactory,
        IConfiguration configuration, RuleKeyExtractorParser keyExtractorParser)
    {
        _requestMatchersParser = requestMatchersParser;
        _conditionMatcherParser = conditionMatcherParser;
        _fileSectionDeclaredItemsParser = fileSectionDeclaredItemsParser;
        _middlewaresParser = middlewaresParser;
        _functionFactoryCSharpFactory = functionFactoryCSharpFactory;
        _configuration = configuration;
        _keyExtractorParser = keyExtractorParser;
    }

    public async Task Load(
        IRequestHandlerBuilder builder,
        string ruleFile,
        IReadonlyParsingContext parsingContext)
    {
        var sectionsRoot = await SectionFileParser.Parse(ruleFile);

        var sections = sectionsRoot.Sections;
        AssertContainsOnlySections(sections,
            [Constants.SectionName.Rule, Constants.SectionName.Declare, Constants.SectionName.Options]);

        var usingContext = _functionFactoryCSharpFactory.CreateRulesUsingContext(sectionsRoot.Lines);

        var ctx = new ParsingContext(
            parsingContext,
            cSharpUsingContext: usingContext,
            currentPath: ruleFile.GetDirectory());

        var declaredItems = await GetDeclaredItems(sections, ctx);
        ctx.SetDeclaredItems(declaredItems);

        var ruleSections = sections.Where(s => s.Name == Constants.SectionName.Rule).ToArray();
        for (var i = 0; i < ruleSections.Length; i++)
        {
            var fi = new FileInfo(ruleFile);
            var ruleInfo = $"no. {i + 1} file: {fi.FullName}";

            var ruleSection = ruleSections[i];

            await CreateRules(
                builder,
                ruleInfo,
                ruleSection,
                ctx);
        }
    }

    private async Task CreateRules(
        IRequestHandlerBuilder builder,
        string ruleInfo,
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

        var modes = await GetResponseMiddlewareModes(childSections, ctx);

        var cachingEnabled = modes.Caching is CachingMode.Enabled;

        var existsConditionSection = childSections.Any(x => x.Name == Constants.SectionName.Condition);
        if (existsConditionSection && cachingEnabled)
            throw new Exception(
                $"Section '{Constants.SectionName.Condition}' cannot be determined when '{AttributeExtractor.Extract<Options, ParameterNameAttribute>(x => x.CachingEnabled)} = true'");

        if (existsConditionSection)
        {
            AssertContainsOnlySections(childSections, [
                Constants.SectionName.Condition,
                Constants.SectionName.Declare,
                Constants.SectionName.Options
            ]);

            var conditionSections = childSections.Where(s => s.Name == Constants.SectionName.Condition).ToArray();

            if (conditionSections.Length < 2)
                throw new Exception($"Must be at least 2 '{Constants.SectionName.Condition}' sections");

            for (var i = 0; i < conditionSections.Length; i++)
            {
                var conditionSection = conditionSections[i];
                var childConditionSections = conditionSection.ChildSections;
                AssertContainsOnlySections(
                    rulesSections: childConditionSections,
                    expectedSectionName: _middlewaresParser.GetAllSectionNames(childConditionSections)
                        .NewWith(Constants.SectionName.Response, Constants.SectionName.Declare));


                ctx.SetDeclaredItems(await GetDeclaredItems(childConditionSections, ctx));

                var middlewares = await _middlewaresParser.Parse(
                    modes,
                    childConditionSections,
                    ctx);

                var conditionMatchers = _conditionMatcherParser.Parse(conditionSection);

                var matchers = new List<IRequestMatcher>();
                matchers.AddRange(requestMatchers);
                matchers.AddRange(conditionMatchers);

                builder.AddRule(ruleInfo + $". Condition no. {i + 1}", matchers, middlewares);
            }
        }
        else
        {
            var middlewares = await _middlewaresParser.Parse(modes, childSections, ctx);

            AssertContainsOnlySections(
                childSections,
                _middlewaresParser.GetAllSectionNames(childSections)
                    .NewWith(Constants.SectionName.Declare, Constants.SectionName.Options));

            builder.AddRule(ruleInfo, requestMatchers, middlewares);
        }
    }

    private async Task<ResponseMiddlewareModes> GetResponseMiddlewareModes(IImmutableList<FileSection> childSections, IParsingContext parsingContext)
    {
        var options = OptionsSectionParser.Parse(childSections);

        var writeHistoryMode = GetWriteHistoryMode(options);
        var cachingMode = await GetCachingMode(options, parsingContext);

        return new ResponseMiddlewareModes(cachingMode, writeHistoryMode);
    }

    private async Task<CachingMode> GetCachingMode(Options? options, IParsingContext parsingContext)
    {
        CachingMode cachingMode;
        if (string.IsNullOrWhiteSpace(options?.CachingEnabled))
        {
            cachingMode = CachingMode.Disabled.Instance;
        }
        else
        {
            var lifeTime = bool.TryParse(options.CachingEnabled, out _)
                ? _configuration.GetValue<TimeSpan>("DefaultLifeTimeCachingResponse")
                : PrettyTimespanParser.Parse(options.CachingEnabled);

            var extractRuleKeyExpression = options.RuleKey;

            if (string.IsNullOrWhiteSpace(extractRuleKeyExpression))
                throw new Exception($"If caching is enabled, the '{AttributeExtractor.Extract<Options, ParameterNameAttribute>(x => x.RuleKey)}' parameter is required");

            var keyExtractor = await _keyExtractorParser.Parse(extractRuleKeyExpression, parsingContext);
            cachingMode = new CachingMode.Enabled(lifeTime, keyExtractor);
        }

        return cachingMode;
    }

    private static WriteHistoryMode GetWriteHistoryMode(Options? config)
    {
        WriteHistoryMode writeHistoryMode;
        if (config?.HistoryEnabled == true)
        {
            if (config.RuleName == null)
                throw new Exception(
                    $"If parameter '{AttributeExtractor.Extract<Options, ParameterNameAttribute>(opt => opt.HistoryEnabled).Name}' set true, " +
                    $"you must set parameter '{AttributeExtractor.Extract<Options, ParameterNameAttribute>(opt => opt.RuleName).Name}'");

            writeHistoryMode = new WriteHistoryMode.Write(new RuleName(config.RuleName));
        }
        else
        {
            writeHistoryMode = WriteHistoryMode.None.Instance;
        }

        return writeHistoryMode;
    }

    private async Task<DeclaredItems> GetDeclaredItems(IReadOnlyCollection<FileSection> childSections,
        ParsingContext parsingContext)
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