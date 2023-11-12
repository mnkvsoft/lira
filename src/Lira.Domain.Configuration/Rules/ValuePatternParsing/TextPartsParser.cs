using System.Text;
using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Transform;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface ITextPartsParser
{
    Task<ObjectTextParts> Parse(string pattern, IParsingContext parsingContext);
}

class TextPartsParser : ITextPartsParser
{
    private record Static(object Value) : IObjectTextPart
    {
        public object Get(RequestData request) => Value;
    }

    private readonly IGeneratingFunctionFactory _generatingFunctionFactory;
    private readonly IGeneratingCSharpFactory _generatingCSharpFactory;
    private readonly ITransformFunctionFactory _transformFunctionFactory;
    private readonly ILogger _logger;
    
    public TextPartsParser(
        IGeneratingFunctionFactory generatingFunctionFactory, 
        ITransformFunctionFactory transformFunctionFactory, 
        IGeneratingCSharpFactory generatingCSharpFactory,
        ILoggerFactory loggerFactory)
    {
        _generatingFunctionFactory = generatingFunctionFactory;
        _transformFunctionFactory = transformFunctionFactory;
        _generatingCSharpFactory = generatingCSharpFactory;
        _logger = loggerFactory.CreateLogger(GetType());
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

        TransformPipeline pipeline;
        if (value.Contains("return"))
        {
            var csharpFunction = _generatingCSharpFactory.Create(
                new DeclaredPartsProvider(context.DeclaredItems), 
                value);
            
            pipeline = new TransformPipeline(csharpFunction);
        }
        else
        {
            var pipelineItemsRaw = value.Split(Consts.ControlChars.PipelineSplitter);

            pipeline = new TransformPipeline(CreateStartFunction(pipelineItemsRaw[0].Trim(), context));

            for (int i = 1; i < pipelineItemsRaw.Length; i++)
            {
                var invoke = pipelineItemsRaw[i].Trim();
                if (_transformFunctionFactory.TryCreate(invoke, out var transformFunction))
                {
                    pipeline.Add(transformFunction);
                }
                else
                {
                    pipeline.Add(_generatingCSharpFactory.CreateTransform(invoke));
                }
            }    
        }

        return new[] { pipeline };
    }
    
    private IObjectTextPart CreateStartFunction(string rawText, ParsingContext context)
    {
        if (ContainsOnlyVariable(rawText))
        {
            var varName = rawText.TrimStart(Consts.ControlChars.VariablePrefix);
            var variable = context.DeclaredItems.Variables.GetOrThrow(varName);
            return variable;
        }

        var declaredFunction = context.DeclaredItems.Functions.FirstOrDefault(x => x.Name == rawText.TrimStart(Consts.ControlChars.FunctionPrefix));

        if (_generatingFunctionFactory.TryCreate(rawText, out var prettyFunction))
        {
            if (declaredFunction != null)
            {
                _logger.LogInformation($"System function '{rawText}' was replaced by custom declared");
                return declaredFunction;
            }
            
            return prettyFunction;
        }

        if (declaredFunction != null)
            return declaredFunction;

        return _generatingCSharpFactory.Create(
            new DeclaredPartsProvider(context.DeclaredItems), 
            rawText);
        
        bool ContainsOnlyVariable(string s)
        {
            return s.StartsWith(Consts.ControlChars.VariablePrefix) && CustomItemName.IsValidName(s.TrimStart(Consts.ControlChars.VariablePrefix));
        }
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