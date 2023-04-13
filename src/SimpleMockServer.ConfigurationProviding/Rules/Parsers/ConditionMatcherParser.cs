using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules.Parsers;

class ConditionMatcherParser
{
    private readonly FunctionFactory _functionFactory;
    private readonly IRequestStatisticStorage _requestStatisticStorage;

    public ConditionMatcherParser(FunctionFactory functionFactory, IRequestStatisticStorage requestStatisticStorage)
    {
        _functionFactory = functionFactory;
        _requestStatisticStorage = requestStatisticStorage;
    }

    static class ConditionMatcherName
    {
        public const string Attempt = "attempt";
    }

    public ConditionMatcherSet Parse(FileSection conditionSection)
    {
        return new ConditionMatcherSet(_requestStatisticStorage, conditionSection.LinesWithoutBlock.Select(CreateConditionMatcher).ToArray());
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
}
