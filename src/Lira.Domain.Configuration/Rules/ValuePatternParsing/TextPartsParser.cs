using Lira.Common.Exceptions;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
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
            parts.Add(await CreateValuePart(patternPart, localContext));
        }

        var ctx = parsingContext.ToImpl();
        ctx.DeclaredItemsRegistry.Merge(localContext.DeclaredItemsRegistry);

        bool isString = patternParts.Count == 0 || patternParts.Count > 1 || patternParts[0] is PatternPart.Static;
        return new ObjectTextParts(parts, isString);
    }

    private async Task<IObjectTextPart> CreateValuePart(
        PatternPart patternPart,
        ParsingContext context)
    {
        return patternPart switch
        {
            PatternPart.Static @static => new Static(@static.Value),
            PatternPart.Dynamic dynamic => await GetDynamicParts(dynamic, context),
            _ => throw new UnsupportedInstanceType(patternPart)
        };
    }

    private async Task<IObjectTextPart> GetDynamicParts(
        PatternPart.Dynamic dynamicPart,
        ParsingContext context)
    {
        string value = dynamicPart.Value;

        TransformPipeline pipeline;
        if (value.Contains("return"))
        {
            var functionFactory = await _functionFactoryCSharpFactory.Get();
            var createFunctionResult = functionFactory.TryCreateGeneratingFunction(
                new FunctionFactoryRuleContext(context.CSharpUsingContext,
                    new DeclaredPartsProvider(context.DeclaredItemsRegistry)),
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

                var variableReference = context.DeclaredItemsRegistry.TryGetVariable(maybeVariableDeclaration, startFunction.ReturnType);

                if(variableReference != null)
                    return new ObjectTextPartWithSaveVariable(startFunction, variableReference);
            }

            for (int i = 1; i < pipelineItemsRaw.Length; i++)
            {
                var invoke = pipelineItemsRaw[i].Trim();
                pipeline.Add(await CreateTransformFunction(invoke, context));
            }
        }

        return pipeline;
    }

    private static CodeBlock GetCodeBlock(ParsingContext context, string value)
    {
        var (codeBlock, newRuntimeVariables, localVariables) = CodeParser.Parse(value, context.DeclaredItemsRegistry.GetAllDeclaredNames());

        context.DeclaredItemsRegistry.AddVariablesRange(newRuntimeVariables);
        context.DeclaredItemsRegistry.AddVariablesRange(localVariables);

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
        var declaredItemsRegistry = context.DeclaredItemsRegistry;

        string functionName = rawText.StartsWith(Function.Prefix) ? rawText : Function.Prefix + rawText;
        var functionReference = declaredItemsRegistry.TryGetFunctionReference(functionName);

        context.CustomDicts.TryGetCustomSetFunction(rawText, out var customSetFunction);

        if (_functionFactorySystem.TryCreateGeneratingFunction(rawText, out var function))
        {
            if (functionReference != null)
            {
                _logger.LogInformation($"System function '{rawText}' was replaced by custom declared");
                return functionReference;
            }

            if (customSetFunction != null)
            {
                _logger.LogInformation($"System function '{rawText}' was replaced by custom set");
                return customSetFunction;
            }

            return function;
        }

        if (functionReference != null)
            return functionReference;

        if (customSetFunction != null)
            return customSetFunction;

        var functionFactory = await _functionFactoryCSharpFactory.Get();
        var createFunctionResult = functionFactory.TryCreateGeneratingFunction(
            new FunctionFactoryRuleContext(context.CSharpUsingContext, new DeclaredPartsProvider(declaredItemsRegistry)),
            GetCodeBlock(context, rawText));

        return createFunctionResult.GetFunctionOrThrow(rawText, context);
    }
}