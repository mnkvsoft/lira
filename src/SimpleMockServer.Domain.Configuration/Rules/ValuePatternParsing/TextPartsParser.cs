using System.Globalization;
using System.Text;
using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

public interface ITextPartsParser
{
    Task<ObjectTextParts> Parse(string pattern, IParsingContext parsingContext);
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

    public async Task<ObjectTextParts> Parse(string pattern, IParsingContext parsingContext)
    {
        var patternParts = PatternParser.Parse(pattern);

        var parts = new List<IObjectTextPart>();
        foreach (var patternPart in patternParts)
        {
            parts.AddRange(await CreateValuePart(patternPart, parsingContext));
        }

        return new ObjectTextParts(parts);
    }

    private async Task<IReadOnlyCollection<IObjectTextPart>> CreateValuePart(PatternPart patternPart, IParsingContext parsingContext)
    {
        return patternPart switch
        {
            PatternPart.Static @static => new[] { GetStaticPart(@static) },
            PatternPart.Dynamic dynamic => await GetDynamicParts(dynamic, parsingContext),
            _ => throw new UnsupportedInstanceType(patternPart)
        };
    }

    private async Task<IReadOnlyCollection<IObjectTextPart>> GetDynamicParts(PatternPart.Dynamic dynamicPart, IParsingContext parsingContext)
    {
        string value = dynamicPart.Value;

        var context = parsingContext.ToImpl();

        var (wasRead, parts) = await TryReadParts(context, value);
        if (wasRead)
            return parts!;

        var (invoke, format) = value.SplitToTwoParts(" format:").Trim();

        if (invoke.StartsWith(Constants.ControlChars.VariablePrefix))
        {
            var varName = invoke.TrimStart(Constants.ControlChars.VariablePrefix);

            var variable = context.Variables.GetOrThrow(varName);
            return new[] { WrapToFormattableIfNeed(variable, format) };
        }

        return new[] { WrapToFormattableIfNeed(_generatingFunctionFactory.Create(invoke), format) };
    }

    private async Task<(bool wasRead, IReadOnlyCollection<IObjectTextPart>? parts)> TryReadParts(ParsingContext context, string value)
    {
        if (!value.StartsWith("read.file:"))
            return (false, null);

        var args = value.TrimStart("read.file:");
        var (fileName, encodingName) = args.SplitToTwoParts(" encoding:").Trim();

        string filePath;
        if (fileName.StartsWith('/'))
            filePath = context.RootPath + fileName;
        else
            filePath = Path.Combine(context.CurrentPath, fileName);
        
        var encoding = encodingName == null ? Encoding.UTF8 : Encoding.GetEncoding(encodingName);

        string pattern = await File.ReadAllTextAsync(filePath, encoding);
        var parts = await Parse(pattern, context);
        return (true, parts);
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
