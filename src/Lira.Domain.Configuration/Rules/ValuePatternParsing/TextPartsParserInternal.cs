using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;
using Lira.Domain.TextPart.Impl.System;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;


class TextPartsParserInternal
{
    // public record Context(ParsingContext ParsingContext, LocalVariableSet LocalVariables);

    private readonly IFunctionFactorySystem _functionFactorySystem;

    private readonly IFunctionFactoryCSharpFactory _functionFactoryCSharpFactory;

    private readonly ILogger _logger;


    public TextPartsParserInternal(
        IFunctionFactorySystem functionFactorySystem,
        IFunctionFactoryCSharpFactory functionFactoryCSharpFactory,
        ILoggerFactory loggerFactory)
    {
        _functionFactorySystem = functionFactorySystem;
        _functionFactoryCSharpFactory = functionFactoryCSharpFactory;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<IReadOnlyCollection<IObjectTextPart>> Parse(IReadOnlyList<Token> tokens,
        IParsingContext context, OperatorPartFactory operatorPartFactory)
    {
        var result = new List<IObjectTextPart>();

        foreach (var token in tokens)
        {
            if (token is Token.StaticData sd)
            {
                result.AddRange(await Parse(sd, context));
            }
            else if (token is Token.Operator op)
            {
                result.Add(await operatorPartFactory.CreateOperatorPart(op, context));
            }
            else
            {
                throw new UnsupportedInstanceType(token);
            }
        }

        return result;
    }

    async Task<IReadOnlyCollection<IObjectTextPart>> Parse(Token.StaticData staticData, IParsingContext context)
    {
        var parts = new List<IObjectTextPart>();

        var ctx = context.ToImpl();
        foreach (var part in staticData.Content)
        {
            parts.AddRange(await CreateValuePart(part, ctx));
        }

        return parts;
    }

    private async Task<IReadOnlyCollection<IObjectTextPart>> CreateValuePart(
        PatternPart patternPart,
        ParsingContext context)
    {
        return patternPart switch
        {
            PatternPart.Static @static => [new StaticPart(@static.Value)],
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
                    new DeclaredItemsProvider(context.DeclaredItems)),
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
                if (maybeVariableDeclaration.StartsWith(RuleVariable.Prefix))
                {
                    if (maybeVariableDeclaration.Split(" ").Length > 1)
                        throw new Exception(
                            $"Invalid write to variable declaration: {Consts.ControlChars.WriteToVariablePrefix} " +
                            maybeVariableDeclaration);

                    var existVar = context.DeclaredItems.OfType<Variable>()
                        .SingleOrDefault(x => x.Name == maybeVariableDeclaration);

                    if (existVar == null)
                    {
                        var variable = new RuntimeRuleVariable(
                            maybeVariableDeclaration,
                            startFunction.ReturnType);

                        context.DeclaredItems.Add(variable);
                        return [new ObjectTextPartWithSaveVariable(startFunction, variable)];
                    }

                    context.DeclaredItems.Add(existVar);
                    return [new ObjectTextPartWithSaveVariable(startFunction, existVar)];
                }

                // case: {{ guid >> $id }}
                if (maybeVariableDeclaration.StartsWith(LocalVariable.Prefix))
                {
                    if (maybeVariableDeclaration.Split(" ").Length > 1)
                        throw new Exception(
                            $"Invalid write to local variable declaration: {Consts.ControlChars.WriteToVariablePrefix} " +
                            maybeVariableDeclaration);

                    var existVar = context.DeclaredItems.OfType<Variable>()
                        .SingleOrDefault(x => x.Name == maybeVariableDeclaration);

                    if (existVar == null)
                    {
                        var variable = new LocalVariable(
                            maybeVariableDeclaration,
                            startFunction.ReturnType);

                        context.DeclaredItems.Add(variable);
                        return [new ObjectTextPartWithSaveVariable(startFunction, variable)];
                    }

                    return [new ObjectTextPartWithSaveVariable(startFunction, existVar)];
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

        context.DeclaredItems.TryAddRange(newRuntimeVariables);
        context.DeclaredItems.TryAddRange(localVariables);

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

        var declaredFunction = declaredItems
            .OfType<Function>()
            .SingleOrDefault(x => x.Name == Function.Prefix + rawText);

        context.CustomDicts.TryGetCustomSetFunction(rawText, out var customSetFunction);

        if (_functionFactorySystem.TryCreateGeneratingFunction(rawText,
                new SystemFunctionContext(new DeclaredItemsProvider(declaredItems)), out var function))
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

        var declaredItem = declaredItems
            .SingleOrDefault(x => x.Name == rawText);

        if (declaredItem != null)
            return declaredItem;

        var functionFactory = await _functionFactoryCSharpFactory.Get();
        var createFunctionResult = functionFactory.TryCreateGeneratingFunction(
            new FunctionFactoryRuleContext(context.CSharpUsingContext, new DeclaredItemsProvider(declaredItems)),
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