using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;
using Lira.Domain.TextPart.Impl.System;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface ITextPartsParser
{
    Task<ObjectTextParts> Parse(string pattern, IParsingContext parsingContext);
}

class TextPartsParser : ITextPartsParser
{
    private record Static(string Value) : IObjectTextPart
    {
        public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(Value);
        public ReturnType ReturnType => ReturnType.String;
    }

    // public record Context(ParsingContext ParsingContext, LocalVariableSet LocalVariables);

    private readonly IFunctionFactorySystem _functionFactorySystem;

    private readonly IFunctionFactoryCSharpFactory _functionFactoryCSharpFactory;

    private readonly ILogger _logger;

    public TextPartsParser(
        IFunctionFactorySystem functionFactorySystem,
        IFunctionFactoryCSharpFactory functionFactoryCSharpFactory,
        ILoggerFactory loggerFactory)
    {
        _functionFactorySystem = functionFactorySystem;
        _functionFactoryCSharpFactory = functionFactoryCSharpFactory;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<ObjectTextParts> Parse(string pattern, IParsingContext parsingContext)
    {
        var patternParts = PatternParser.Parse(pattern);

        var parts = new List<IObjectTextPart>();

        // with local variables context
        var localContext = new ParsingContext(parsingContext.ToImpl());
        foreach (var patternPart in patternParts)
        {
            parts.AddRange(await CreateValuePart(patternPart, localContext));
        }

        var ctx = parsingContext.ToImpl();
        ctx.DeclaredItems.Variables.TryAddRuntimeVariables(localContext.DeclaredItems.Variables.OfType<RuntimeRuleVariable>());

        bool isString = patternParts.Count == 0 || patternParts.Count > 1 || patternParts[0] is PatternPart.Static;
        return new ObjectTextParts(parts, isString);
    }

    private async Task<IReadOnlyCollection<IObjectTextPart>> CreateValuePart(
        PatternPart patternPart,
        ParsingContext context)
    {
        return patternPart switch
        {
            PatternPart.Static @static => [new Static(@static.Value)],
            PatternPart.Dynamic dynamic => await GetDynamicParts(dynamic, context),
            _ => throw new UnsupportedInstanceType(patternPart)
        };
    }

    private async Task<IReadOnlyCollection<IObjectTextPart>> GetDynamicParts(
        PatternPart.Dynamic dynamicPart,
        ParsingContext context)
    {
        string value = dynamicPart.Value;

        // var (wasRead, parts) = await TryReadParts(context, value);
        // if (wasRead)
        //     return parts!;

        TransformPipeline pipeline;
        if (value.Contains("return"))
        {
            var functionFactory = await _functionFactoryCSharpFactory.Get();
            var createFunctionResult = functionFactory.TryCreateGeneratingFunction(
                new FunctionFactoryRuleContext(context.CSharpUsingContext,
                    new DeclaredPartsProvider(context.DeclaredItems)),
                GetCodeBlock(context, value));

            var function = createFunctionResult.GetFunctionOrThrow(value, context);
            pipeline = new TransformPipeline(function);
        }
        else
        {
            var pipelineItemsRaw = value.Split(Consts.ControlChars.PipelineSplitter);

            var startFunction = await CreateStartFunction(pipelineItemsRaw[0].Trim(), context);
            pipeline = new TransformPipeline(startFunction);

            if (pipelineItemsRaw.Length == 2)
            {
                var maybeVariableDeclaration = pipelineItemsRaw[1].Trim();

                // case: {{ guid >> $$id }}
                if (maybeVariableDeclaration.StartsWith(Consts.ControlChars.RuleVariablePrefix))
                {
                    if (maybeVariableDeclaration.Split(" ").Length > 1)
                        throw new Exception(
                            $"Invalid write to variable declaration: {Consts.ControlChars.WriteToVariablePrefix} " +
                            maybeVariableDeclaration);

                    var variable = new RuntimeRuleVariable(
                        new CustomItemName(maybeVariableDeclaration.TrimStart(Consts.ControlChars.RuleVariablePrefix)),
                        startFunction.ReturnType);
                    context.DeclaredItems.Variables.Add(variable);
                    return [new ObjectTextPartWithSaveVariable(startFunction, variable)];
                }

                // case: {{ guid >> $id }}
                if (maybeVariableDeclaration.StartsWith(Consts.ControlChars.LocalVariablePrefix))
                {
                    if (maybeVariableDeclaration.Split(" ").Length > 1)
                        throw new Exception(
                            $"Invalid write to local variable declaration: {Consts.ControlChars.WriteToVariablePrefix} " +
                            maybeVariableDeclaration);

                    var name = new CustomItemName(maybeVariableDeclaration.TrimStart(Consts.ControlChars.LocalVariablePrefix));
                    var localVar = context.DeclaredItems.LocalVariables.SingleOrDefault(x => x.Name == name.Value);

                    if (localVar == null)
                    {
                        var variable =
                            new LocalVariable(
                                name,
                                startFunction.ReturnType);
                        context.DeclaredItems.LocalVariables.Add(variable);
                        return [new ObjectTextPartWithSaveVariable(startFunction, variable)];
                    }

                    return [new ObjectTextPartWithSaveVariable(startFunction, localVar)];
                }
            }

            for (int i = 1; i < pipelineItemsRaw.Length; i++)
            {
                var invoke = pipelineItemsRaw[i].Trim();
                pipeline.Add(await CreateTransformFunction(invoke, context));
            }
        }

        return [pipeline];
    }

    private static CodeBlock GetCodeBlock(ParsingContext context, string value)
    {
        var (codeBlock, newRuntimeVariables, localVariables) = CodeParser.Parse(value, context.DeclaredItems);

        context.DeclaredItems.Variables.TryAddRuntimeVariables(newRuntimeVariables);
        context.DeclaredItems.LocalVariables.TryAddLocalVariables(localVariables);

        return codeBlock;
    }

    private async Task<ITransformFunction> CreateTransformFunction(string invoke, ParsingContext context)
    {
        if (_functionFactorySystem.TryCreateTransformFunction(invoke, out var transformFunction))
            return transformFunction;

        var functionFactory = await _functionFactoryCSharpFactory.Get();
        var createFunctionResult = functionFactory.TryCreateTransformFunction(GetCodeBlock(context, invoke));

        return createFunctionResult.GetFunctionOrThrow(invoke, context);
    }

    private async Task<IObjectTextPart> CreateStartFunction(string rawText, ParsingContext context)
    {
        var declaredItems = context.DeclaredItems;

        var declaredFunction =
            declaredItems.Functions.FirstOrDefault(x =>
                x.Name == rawText.TrimStart(Consts.ControlChars.FunctionPrefix));
        context.CustomDicts.TryGetCustomSetFunction(rawText, out var customSetFunction);

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

        var functionFactory = await _functionFactoryCSharpFactory.Get();
        var createFunctionResult = functionFactory.TryCreateGeneratingFunction(
            new FunctionFactoryRuleContext(context.CSharpUsingContext, new DeclaredPartsProvider(declaredItems)),
            GetCodeBlock(context, rawText));

        return createFunctionResult.GetFunctionOrThrow(rawText, context);
    }

    // private async Task<(bool wasRead, IReadOnlyCollection<IObjectTextPart>? parts)> TryReadParts(
    //     IReadonlyParsingContext context, string invoke)
    // {
    //     var (wasRead, parts) = await TryReadPartsFromFile(context, invoke);
    //
    //     if (wasRead)
    //         return (true, parts!);
    //
    //     (wasRead, parts) = await TryReadPartsFromTemplate(context, invoke);
    //
    //     if (wasRead)
    //         return (true, parts!);
    //
    //     return (false, null);
    // }

    // private async Task<(bool wasRead, IReadOnlyCollection<IObjectTextPart>? parts)> TryReadPartsFromTemplate(
    //     IReadonlyParsingContext context, string invoke)
    // {
    //     if (invoke.StartsWith(Consts.ControlChars.TemplatePrefix))
    //     {
    //         var templateName = invoke.TrimStart(Consts.ControlChars.TemplatePrefix);
    //
    //         var template = context.Templates.GetOrThrow(templateName);
    //
    //         var parts = await Parse(template.Value, context);
    //         return (true, parts);
    //     }
    //
    //     return (false, null);
    // }

    // private async Task<(bool wasRead, IReadOnlyCollection<IObjectTextPart>? parts)> TryReadPartsFromFile(
    //     IReadonlyParsingContext context, string invoke)
    // {
    //     if (!invoke.StartsWith("read.file:"))
    //         return (false, null);
    //
    //     var args = invoke.TrimStart("read.file:");
    //     var (fileName, encodingName) = args.SplitToTwoParts(" encoding:").Trim();
    //
    //     string filePath;
    //     if (fileName.StartsWith('/'))
    //         filePath = context.RootPath + fileName;
    //     else
    //         filePath = Path.Combine(context.CurrentPath, fileName);
    //
    //     var encoding = encodingName == null ? Encoding.UTF8 : Encoding.GetEncoding(encodingName);
    //
    //     string pattern = await File.ReadAllTextAsync(filePath, encoding);
    //     var parts = await Parse(pattern.Replace("\r\n", "\n"), context);
    //     return (true, parts);
    // }
}