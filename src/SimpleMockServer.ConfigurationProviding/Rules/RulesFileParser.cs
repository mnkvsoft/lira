using System.Reflection;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.Parsers;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules;

partial class RulesFileParser
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly FunctionFactory _functionFactory;
    private readonly IRequestStatisticStorage _requestStatisticStorage;
    private readonly RequestMatchersParser _requestMatchersParser;
    private readonly ResponseWriterParser _responseWriterParser;

    public RulesFileParser(
        ILoggerFactory loggerFactory, 
        FunctionFactory functionFactory, 
        IRequestStatisticStorage requestStatisticStorage,
        RequestMatchersParser requestMatchersParser, 
        ResponseWriterParser responseWriterParser)
    {
        _loggerFactory = loggerFactory;
        _functionFactory = functionFactory;
        _requestStatisticStorage = requestStatisticStorage;
        _requestMatchersParser = requestMatchersParser;
        _responseWriterParser = responseWriterParser;
    }

    public async Task<IReadOnlyCollection<Rule>> Parse(string ruleFile)
    {
        try
        {
            var rulesSections = await SectionFileParser.Parse(
                ruleFile,
                knownBlockForSections: new Dictionary<string, IReadOnlySet<string>>
                {
                    { "rule", GetBlockNames<Constants.BlockName.Rule>() },
                    { "response", GetBlockNames<Constants.BlockName.Response>() },
                },
                maxNestingDepth: 3);

            AssertContainsOnlySections(rulesSections, Constants.SectionName.Rule);

            var rules = new List<Rule>();
            for (int i = 0; i < rulesSections.Count; i++)
            {
                var fi = new FileInfo(ruleFile);
                var ruleName = $"no. {i + 1} file: {fi.Name}";

                var ruleSection = rulesSections[i];
                rules.AddRange(CreateRules(ruleName, ruleSection));
            }

            return rules;
        }
        catch (Exception exc)
        {
            throw new Exception("An error occured while parsing file: " + ruleFile, exc);
        }
    }

    private IReadOnlyCollection<Rule> CreateRules(string ruleName, FileSection ruleSection)
    {
        var childSections = ruleSection.ChildSections;

        if (childSections.Count == 0)
            throw new Exception("Rule section is empty");

        var requestMatcherSet = _requestMatchersParser.Parse(ruleSection);

        var section = childSections[0];

        ResponseWriter responseWriter;
        
        if (section.Name == Constants.SectionName.Condition)
        {
            AssertContainsOnlySections(childSections, Constants.SectionName.Condition);

            if (childSections.Count < 2)
                throw new Exception($"Must be at least 2 '{Constants.SectionName.Condition}' sections");

            var rules = new List<Rule>();
            for (var i = 0; i < childSections.Count; i++)
            {
                var conditionSection = childSections[i];
                var childConditionSections = conditionSection.ChildSections;
                AssertContainsOnlySections(childConditionSections, Constants.SectionName.Response, Constants.SectionName.Callback);

                var conditionMatchers = conditionSection.LinesWithoutBlock.Select(CreateConditionMatcher).ToList();

                responseWriter = _responseWriterParser.Parse(conditionSection);
                rules.Add(new Rule(
                    ruleName + $". Condition no. {i + 1}",
                    _loggerFactory,
                    responseWriter,
                    requestMatcherSet,
                    new ConditionMatcherSet(_requestStatisticStorage, conditionMatchers)));
            }

            return rules;
        }

        AssertContainsOnlySections(childSections, Constants.SectionName.Response, Constants.SectionName.Callback);
        responseWriter = _responseWriterParser.Parse(ruleSection);
        return new[] { new Rule(ruleName, _loggerFactory, responseWriter, requestMatcherSet, conditionMatcherSet: null) };
    }

    
    private IConditionMatcher CreateConditionMatcher(string line)
    {
        (string matcherName, string functionInvoke) = line.SplitToTwoPartsRequired(Constants.ManageChar.Lambda).TrimRequired();

        if (matcherName == ConditionMatcherName.Attempt)
        {
            var function = _functionFactory.CreateIntMatchFunction(functionInvoke);
            return new AttemptConditionMatcher(function);
        }

        throw new Exception($"Unknown condition matcher '{matcherName}'");
    }

    private static IReadOnlySet<string> GetBlockNames<T>()
    {
        var set = new HashSet<string>();
        var values = GetAllPublicConstantValues<string>(typeof(T));

        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception("Empty block name");

            set.AddOrThrowIfContains(value);
        }

        return set;
    }

    private static IReadOnlyCollection<T?> GetAllPublicConstantValues<T>(Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
            .Select(x => (T?)x.GetRawConstantValue())
            .ToList();
    }


    private static void AssertContainsOnlySections(IReadOnlyList<FileSection> rulesSections, params string[] expectedSectionName)
    {
        var unknownSections = rulesSections
            .Where(s => !expectedSectionName.Contains(s.Name))
            .Select(x => x.Name)
            .ToArray();

        if (unknownSections.Length != 0)
            throw new Exception("Unknown sections: " + string.Join(", ", unknownSections) + ". Expected: " +
                                string.Join(", ", expectedSectionName));
    }
}
