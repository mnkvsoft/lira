using System.Globalization;
using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

public interface ITextPartsParser
{
    ObjectTextParts Parse(string pattern, IReadOnlyCollection<Variable> variables);
}

class TextPartsParser : ITextPartsParser
{
    public record Static(object Value) : IGlobalObjectTextPart
    {
        public object Get(RequestData request) => Value;

        public object Get() => Value;
    }

    private readonly IGeneratingFunctionFactory _generatingFunctionFactory;

    public TextPartsParser(IGeneratingFunctionFactory generatingFunctionFactory)
    {
        _generatingFunctionFactory = generatingFunctionFactory;
    }

    public ObjectTextParts Parse(string pattern, IReadOnlyCollection<Variable> variables)
    {
        var patternParts = PatternParser.Parse(pattern);

        var parts = new List<IObjectTextPart>();
        foreach (var patternPart in patternParts)
        {
            parts.Add(CreateValuePart(patternPart, variables));
        }

        return new ObjectTextParts(parts);
    }

    private IObjectTextPart CreateValuePart(PatternPart patternPart, IReadOnlyCollection<Variable> variables)
    {
        return patternPart switch
        {
            PatternPart.Static @static => GetStaticPart(@static),
            PatternPart.Dynamic dynamic => GetDynamicPart(dynamic, variables),
            _ => throw new UnsupportedInstanceType(patternPart)
        };
    }

    private IObjectTextPart GetDynamicPart(PatternPart.Dynamic dynamicPart, IReadOnlyCollection<Variable> variables)
    {
        string value = dynamicPart.Value;
        
        var (invoke, format) = value.SplitToTwoParts(" format:").Trim();
        
        if (invoke.StartsWith(Constants.ControlChars.VariablePrefix))
        {
            var varName = invoke.TrimStart(Constants.ControlChars.VariablePrefix);
            var variable = variables.GetOrThrow(varName);
            return WrapToFormattableIfNeed(variable, format);
        }

        return WrapToFormattableIfNeed(_generatingFunctionFactory.Create(invoke), format);
    }

    private IObjectTextPart GetStaticPart(PatternPart.Static staticPart)
    {
        string value = staticPart.Value;

        if (long.TryParse(value, CultureInfo.InvariantCulture, out var longValue))
            return new Static(longValue);
        
        if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var decimalValue))
            return new Static(decimalValue);
        
        if (Guid.TryParse(value, CultureInfo.InvariantCulture, out var guidValue))
            return new Static(guidValue);
        
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, out var dateValue))
            return new Static(dateValue);
        
        if (bool.TryParse(value, out var boolValue))
            return new Static(boolValue);

        return new Static(value);
    }
    
    private static IObjectTextPart WrapToFormattableIfNeed(IObjectTextPart objectTextPart, string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return objectTextPart;
        
        if (objectTextPart is IGlobalObjectTextPart globalObjectTextPart)
            return new GlobalFormattableTextPart(globalObjectTextPart, format);

        return new FormattableTextPart(objectTextPart, format);
    }
}
