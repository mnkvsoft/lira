using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Models.RulesModel.Generating;

namespace SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;

class ValuePartsCreator
{
    private readonly FunctionFactory _functionFactory;

    public ValuePartsCreator(FunctionFactory functionFactory)
    {
        _functionFactory = functionFactory;
    }

    public IReadOnlyCollection<ValuePart> Create(string pattern, VariableSet variables)
    {
        var patternParts = PatternParser.Parse(pattern);

        var valueParts = new List<ValuePart>();
        foreach (var patternPart in patternParts)
        {
            valueParts.Add(CreateValuePart(patternPart, variables));
        }

        return valueParts;
    }

    private ValuePart CreateValuePart(PatternPart patternPart, VariableSet variables)
    {
        switch (patternPart)
        {
            case PatternPart.Static:
                return new ValuePart.Static(patternPart.Value);
            case PatternPart.Dynamic dynamicPart:
                return GetDynamicPart(dynamicPart, variables);
            default:
                throw new UnsupportedInstanceType(patternPart);
        }
    }

    private ValuePart GetDynamicPart(PatternPart.Dynamic dynamicPart, VariableSet variables)
    {
        if (dynamicPart.Value.StartsWith(Constants.ControlChars.VariablePrefix))
        {
            var varName = dynamicPart.Value.TrimStart(Constants.ControlChars.VariablePrefix);
            var variable = variables.GetOrThrow(varName);
            return variable;
        }

        return new ValuePart.Function(_functionFactory.CreateGeneratingFunction(dynamicPart.Value));
    }
}
