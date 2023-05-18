using System.Text;
using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing.Extensions;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Transform;

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
    private readonly ITransformFunctionFactory _transformFunctionFactory;

    public TextPartsParser(IGeneratingFunctionFactory generatingFunctionFactory, ITransformFunctionFactory transformFunctionFactory)
    {
        _generatingFunctionFactory = generatingFunctionFactory;
        _transformFunctionFactory = transformFunctionFactory;
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
            PatternPart.Static @static => new[] { new Static(@static.Value) },
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

        if (value.Contains("return"))
        {
            throw new NotImplementedException();
        }
        
        var pipelineItemsRaw = value.Split(Consts.ControlChars.PipelineSplitter);

        var pipeline = CreatePipeline(CreateStartFunction(pipelineItemsRaw[0].Trim(), context));

        for (int i = 1; i < pipelineItemsRaw.Length; i++)
        {
            if(_transformFunctionFactory.TryCreate(pipelineItemsRaw[i].Trim(), out var transformFunction))
                pipeline.Add(transformFunction);
        }

        return new[] { pipeline };
    }

    private TransformPipelineBase CreatePipeline(IObjectTextPart startFunction)
    {
        if (startFunction is IGlobalObjectTextPart globalObjectTextPart)
            return new GlobalTransformPipeline(globalObjectTextPart);
        return new TransformPipeline(startFunction);
    }
    
    private IObjectTextPart CreateStartFunction(string rawText, ParsingContext context)
    {
        if (rawText.StartsWith(Consts.ControlChars.VariablePrefix))
        {
            var varName = rawText.TrimStart(Consts.ControlChars.VariablePrefix);

            var variable = context.Variables.GetOrThrow(varName);
            return variable;
        }
        
        if (_generatingFunctionFactory.TryCreate(rawText, out var prettyFunction))
            return prettyFunction;

        throw new NotImplementedException();
    }
    

    private async Task<(bool wasRead, IReadOnlyCollection<IObjectTextPart>? parts)> TryReadParts(
        ParsingContext context, string invoke)
    {
        var (wasRead, parts) = await TryReadPartsFromFile(context, invoke);
        
        if (wasRead)
            return (true, parts!);
        
        (wasRead, parts) = await TryReadPartsFromTemplate(context, invoke);
        
        if (wasRead)
            return (true, parts!);

        return (false, null);
    }

    private async Task<(bool wasRead, IReadOnlyCollection<IObjectTextPart>? parts)> TryReadPartsFromTemplate(
        ParsingContext context, string invoke)
    {
        if (invoke.StartsWith(Consts.ControlChars.TemplatePrefix))
        {
            var templateName = invoke.TrimStart(Consts.ControlChars.TemplatePrefix);

            var template = context.Templates.GetOrThrow(templateName);
            
            var parts = await Parse(template.Value, context);
            return (true, parts);
        }
        
        return (false, null);
    }
    
    private async Task<(bool wasRead, IReadOnlyCollection<IObjectTextPart>? parts)> TryReadPartsFromFile(
        ParsingContext context, string invoke)
    {
        if (!invoke.StartsWith("read.file:"))
            return (false, null);

        var args = invoke.TrimStart("read.file:");
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
}
