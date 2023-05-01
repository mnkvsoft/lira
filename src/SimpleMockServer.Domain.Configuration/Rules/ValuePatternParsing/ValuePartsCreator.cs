using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Generating;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

public interface ITextPartsParser
{
    TextParts Parse(string pattern, IReadOnlyCollection<Variable> variables);
}

class TextPartsParser : ITextPartsParser
{
    public record Static(string Value) : IGlobalTextPart
    {
        public string? Get(RequestData request) => Value;

        public string? Get() => Value;
    }

    private readonly IGeneratingFunctionFactory _generatingFunctionFactory;

    public TextPartsParser(IGeneratingFunctionFactory generatingFunctionFactory)
    {
        _generatingFunctionFactory = generatingFunctionFactory;
    }

    public TextParts Parse(string pattern, IReadOnlyCollection<Variable> variables)
    {
        var patternParts = PatternParser.Parse(pattern);

        var parts = new List<ITextPart>();
        foreach (var patternPart in patternParts)
        {
            parts.Add(CreateValuePart(patternPart, variables));
        }

        return new TextParts(parts);
    }

    private ITextPart CreateValuePart(PatternPart patternPart, IReadOnlyCollection<Variable> variables)
    {
        switch (patternPart)
        {
            case PatternPart.Static:
                return new Static(patternPart.Value);
            case PatternPart.Dynamic dynamicPart:
                return GetDynamicPart(dynamicPart, variables);
            default:
                throw new UnsupportedInstanceType(patternPart);
        }
    }

    private ITextPart GetDynamicPart(PatternPart.Dynamic dynamicPart, IReadOnlyCollection<Variable> variables)
    {
        if (dynamicPart.Value.StartsWith(Constants.ControlChars.VariablePrefix))
        {
            var varName = dynamicPart.Value.TrimStart(Constants.ControlChars.VariablePrefix);
            var variable = variables.GetOrThrow(varName);
            return variable;
        }

        return _generatingFunctionFactory.Create(dynamicPart.Value);
    }
}
