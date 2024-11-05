using System.Text;
using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;
using Lira.Domain.TextPart.Impl.System;
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
        public dynamic Get(RuleExecutingContext context) => Value;
    }

    private readonly IFunctionFactorySystem _functionFactorySystem;
    private readonly IFunctionFactoryCSharp _functionFactoryCSharp;
    private readonly ILogger _logger;

    public TextPartsParser(
        IFunctionFactorySystem functionFactorySystem,
        IFunctionFactoryCSharp functionFactoryCSharp,
        ILoggerFactory loggerFactory)
    {
        _functionFactorySystem = functionFactorySystem;
        _functionFactoryCSharp = functionFactoryCSharp;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<ObjectTextParts> Parse(string pattern, IParsingContext parsingContext)
    {
        var patternParts = PatternParser.Parse(pattern);

        var parts = new List<IObjectTextPart>();
        foreach (var patternPart in patternParts)
        {
            parts.AddRange(await CreateValuePart(patternPart, parsingContext.ToImpl()));
        }

        return new ObjectTextParts(parts);
    }

    private async Task<IReadOnlyCollection<IObjectTextPart>> CreateValuePart(PatternPart patternPart, ParsingContext parsingContext)
    {
        return patternPart switch
        {
            PatternPart.Static @static => new[] { new Static(@static.Value) },
            PatternPart.Dynamic dynamic => await GetDynamicParts(dynamic, parsingContext),
            _ => throw new UnsupportedInstanceType(patternPart)
        };
    }

    private async Task<IReadOnlyCollection<IObjectTextPart>> GetDynamicParts(PatternPart.Dynamic dynamicPart, ParsingContext context)
    {
        string value = dynamicPart.Value;

        var (wasRead, parts) = await TryReadParts(context, value);
        if (wasRead)
            return parts!;

        TransformPipeline pipeline;
        if (value.Contains("return"))
        {
            var createFunctionResult = _functionFactoryCSharp.TryCreateGeneratingFunction(
                new DeclaredPartsProvider(context.DeclaredItems),
                value);

            if (createFunctionResult is CreateFunctionResult<IObjectTextPart>.Failed failed)
                throw failed.Exception;

            pipeline = new TransformPipeline(((CreateFunctionResult<IObjectTextPart>.Success)createFunctionResult).Function);
        }
        else
        {
            var pipelineItemsRaw = value.Split(Consts.ControlChars.PipelineSplitter);

            pipeline = new TransformPipeline(CreateStartFunction(pipelineItemsRaw[0].Trim(), context));

            for (int i = 1; i < pipelineItemsRaw.Length; i++)
            {
                var invoke = pipelineItemsRaw[i].Trim();
                pipeline.Add(CreateTransformFunction(invoke, context));
            }
        }

        return new[] { pipeline };
    }

    private ITransformFunction CreateTransformFunction(string invoke, ParsingContext context)
    {
        if (_functionFactorySystem.TryCreateTransformFunction(invoke, out var transformFunction))
            return transformFunction;

        var createFunctionResult = _functionFactoryCSharp.TryCreateTransformFunction(invoke);

        return createFunctionResult.GetFunctionOrThrow(invoke, context);
    }

    private IObjectTextPart CreateStartFunction(string rawText, ParsingContext context)
    {
        var declaredItems = context.DeclaredItems;

        if (ContainsOnlyVariable(rawText))
        {
            var varName = rawText.TrimStart(Consts.ControlChars.VariablePrefix);
            var variable = declaredItems.Variables.GetOrThrow(varName);
            return variable;
        }

        var declaredFunction = declaredItems.Functions.FirstOrDefault(x => x.Name == rawText.TrimStart(Consts.ControlChars.FunctionPrefix));
        context.CustomSets.TryGetCustomSetFunction(rawText, out var customSetFunction);

        if (_functionFactorySystem.TryCreateGeneratingFunction(rawText, out var function))
        {
            if (declaredFunction != null)
            {
                _logger.LogInformation($"System function '{rawText}' was replaced by custom declared");
                return declaredFunction;
            }

            if (customSetFunction != null)
            {
                _logger.LogInformation($"System function '{rawText}' was replaced by custom set");
                return customSetFunction;
            }

            return function;
        }

        if (declaredFunction != null)
            return declaredFunction;

        if (customSetFunction != null)
            return customSetFunction;

        var createFunctionResult = _functionFactoryCSharp.TryCreateGeneratingFunction(
            new DeclaredPartsProvider(declaredItems),
            rawText);

        return createFunctionResult.GetFunctionOrThrow(rawText, context);

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
        var parts = await Parse(pattern.Replace("\r\n", "\n"), context);
        return (true, parts);
    }
}