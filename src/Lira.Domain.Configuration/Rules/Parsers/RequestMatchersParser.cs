using System.Web;
using Lira.Domain.Matching.Request;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.Impl;
using Lira.Domain.TextPart.Impl.System;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class RequestMatchersParser
{
    private readonly IFunctionFactorySystem _functionFactorySystem;
    private readonly IFunctionFactoryCSharp _functionFactoryCSharp;

    public RequestMatchersParser(
        IFunctionFactorySystem functionFactorySystem,
        IFunctionFactoryCSharp functionFactoryCSharp)
    {
        _functionFactorySystem = functionFactorySystem;
        _functionFactoryCSharp = functionFactoryCSharp;
    }

    public IReadOnlyCollection<IRequestMatcher> Parse(FileSection ruleSection, ParsingContext context)
    {
        var builder = new RequestMatchersBuilder();

        var matchers = GetMethodAndPathMatchersFromShortEntry(ruleSection, context);
        builder.AddRange(matchers);

        ruleSection.AssertContainsOnlyKnownBlocks(BlockNameHelper.GetBlockNames<Constants.BlockName.Rule>());

        foreach (var block in ruleSection.Blocks)
        {
            if (block.Name == Constants.BlockName.Rule.Path)
            {
                var path = block.GetSingleLine();
                var requestMatcher = CreatePathRequestMatcher(PatternParser.Parse(path), context);

                builder.Add(requestMatcher);
                continue;
            }

            builder.Add(CreateRequestMatcher(block, context));
        }

        return builder.Matchers;
    }

    private IReadOnlyCollection<IRequestMatcher> GetMethodAndPathMatchersFromShortEntry(
        FileSection ruleSection,
        ParsingContext context)
    {
        var lines = ruleSection.LinesWithoutBlock;

        if (lines.Count == 0)
            return Array.Empty<IRequestMatcher>();

        var result = new List<IRequestMatcher>();

        var allPatterns = PatternParser.Parse(lines);

        var (method, startPath) = allPatterns.SplitToTwoPartsRequired(" ");

        var (path, query) = startPath.SplitToTwoParts("?");

        result.Add(CreateMethodRequestMather(method.SingleStaticValueToString()));

        var pathMatcher = CreatePathRequestMatcher(path, context);
        result.Add(pathMatcher);

        if (query != null)
            result.Add(CreateQueryStringMatcher(query, context));

        return result;
    }

    private IRequestMatcher CreateRequestMatcher(FileBlock block, ParsingContext context)
    {
        switch (block.Name)
        {
            case Constants.BlockName.Rule.Method:
                return CreateMethodRequestMather(block.GetSingleLine());
            case Constants.BlockName.Rule.Query:
                return CreateQueryStringMatcher(PatternParser.Parse(block.GetSingleLine()), context);
            case Constants.BlockName.Rule.Headers:
                return CreateHeadersRequestMatcher(block, context);
            case Constants.BlockName.Rule.Body:
                return CreateBodyRequestMatcher(block, context);
            case Constants.BlockName.Rule.Path:
                return CreatePathRequestMatcher(PatternParser.Parse(block.GetSingleLine()), context);
            case Constants.BlockName.Rule.Request:
                return CreateCustomRequestMatcher(block.GetLinesAsString(), context);
            default:
                throw new Exception($"Unknown block '{block.Name}' in 'rule' section");
        }
    }

    private IRequestMatcher CreateCustomRequestMatcher(string code, ParsingContext context)
    {
        var (codeBlock, newRuntimeVariables) = CodeParser.Parse(code, context.DeclaredItems);
        context.DeclaredItems.Variables.AddRange(newRuntimeVariables);

        var matcher = _functionFactoryCSharp.TryCreateRequestMatcher(new DeclaredPartsProvider(context.DeclaredItems), codeBlock);
        return matcher.GetFunctionOrThrow(code, context);
    }

    private static MethodRequestMatcher CreateMethodRequestMather(string method)
    {
        return new MethodRequestMatcher(method.ToHttpMethod());
    }

    private PathRequestMatcher CreatePathRequestMatcher(PatternParts pathParts, ParsingContext context)
    {
        if (pathParts.Count == 0)
            throw new Exception("An error occurred while creating PathRequestMatcher. Path is empty");

        if (pathParts.First() is not PatternPart.Static @static || !@static.Value.StartsWith('/'))
            throw new Exception($"Matching path must start with '/'. Current value: '{pathParts}'");

        var segments = pathParts.Split("/");

        var patterns = new List<TextPatternPart>(segments.Count);

        foreach (var segment in segments)
        {
            patterns.Add(CreateValuePattern(segment, context));
        }

        return new PathRequestMatcher(patterns);
    }

    private QueryStringRequestMatcher CreateQueryStringMatcher(PatternParts queryParts, ParsingContext context)
    {
        var keysWithValueRaw = queryParts.Split("&");
        var patterns = new Dictionary<string, TextPatternPart>();

        foreach (var keyWithValueRaw in keysWithValueRaw)
        {
            var (key, value) = keyWithValueRaw.SplitToTwoParts("=");
            value = value?.Replace(
                p => p is PatternPart.Static,
                p => new PatternPart.Static(HttpUtility.UrlDecode(((PatternPart.Static)p).Value)));

            patterns.Add(key.SingleStaticValueToString(), CreateValuePattern(value, context));
        }

        return new QueryStringRequestMatcher(patterns);
    }

    private HeadersRequestMatcher CreateHeadersRequestMatcher(FileBlock block, ParsingContext context)
    {
        var headers = new Dictionary<string, TextPatternPart>();
        var patterns = PatternParser.Parse(block.Lines);


        foreach (var line in patterns.GetLines())
        {
            var (headerName, headerPattern) = line.SplitToTwoPartsRequired(Consts.ControlChars.HeaderSplitter).Trim();

            headers.Add(headerName.SingleStaticValueToString(), CreateValuePattern(headerPattern, context));
        }

        return new HeadersRequestMatcher(headers);
    }

    private IRequestMatcher CreateBodyRequestMatcher(FileBlock block, ParsingContext context)
    {
        var patterns = new List<KeyValuePair<IBodyExtractFunction, TextPatternPart>>();

        var parts = PatternParser.Parse(block.Lines);
        var lines = parts.GetLines();

        foreach (var line in lines)
        {
            if (!line.ContainsInStatic(Consts.ControlChars.PipelineSplitter))
            {
                if (!_functionFactorySystem.TryCreateBodyExtractFunction(FunctionName.ExtractBody.All,
                        out var function))
                    throw new InvalidOperationException(
                        $"Cannot create system function extract body function '{FunctionName.ExtractBody.All}'");

                patterns.Add(
                    new KeyValuePair<IBodyExtractFunction, TextPatternPart>(function,
                        CreateValuePattern(line, context)));
                continue;
            }

            var (extractFunctionInvoke, pattern) =
                line.SplitToTwoPartsRequired(Consts.ControlChars.PipelineSplitter).Trim();

            // can write either
            // {{ xpath://employee[1]/text() }}
            // or
            // xpath://employee[1]/text()
            var extractFunctionInvokeStr = extractFunctionInvoke.GetSingleDynamic().Value.Trim();

            if (!_functionFactorySystem.TryCreateBodyExtractFunction(extractFunctionInvokeStr,
                    out var bodyExtractFunction))
                throw new Exception($"System function '{extractFunctionInvokeStr}' not found");

            patterns.Add(new KeyValuePair<IBodyExtractFunction, TextPatternPart>(bodyExtractFunction,
                CreateValuePattern(pattern, context)));
        }

        return new BodyRequestMatcher(patterns);
    }

    private TextPatternPart CreateValuePattern(
        PatternParts? parts,
        ParsingContext context)
    {
        if (parts == null || parts.Count == 0)
            return new TextPatternPart.NullOrEmpty();

        if (parts.Count == 1)
        {
            if (parts[0] is PatternPart.Static stat)
                return new TextPatternPart.Static(stat.Value);

            if (parts[0] is PatternPart.Dynamic dyn)
            {
                return new TextPatternPart.Dynamic(Start: null, End: null, CreateMatchFunctionWithSaveVariable(dyn.Value, context));
            }
        }

        if (parts.Count == 2)
        {
            if (!parts.Any(x => x is PatternPart.Static))
                throw new Exception($"'{parts}' must contains one static part");

            if (!parts.Any(x => x is PatternPart.Static))
                throw new Exception($"'{parts}' must contains one dynamic part");

            if (parts[0] is PatternPart.Static @static)
            {
                PatternPart.Dynamic dyn = ((PatternPart.Dynamic)parts[1]);

                return new TextPatternPart.Dynamic(
                    Start: @static.Value,
                    End: null,
                    CreateMatchFunctionWithSaveVariable(dyn.Value, context));
            }

            if (parts[0] is PatternPart.Static dynamic)
            {
                return new TextPatternPart.Dynamic(
                    Start: null,
                    End: ((PatternPart.Static)parts[1]).Value,
                    CreateMatchFunctionWithSaveVariable(dynamic.Value, context));
            }
        }

        if (parts.Count == 3)
        {
            if (parts[0] is not PatternPart.Static start)
                throw new Exception($"First part must be static. Current value: {parts}");

            if (parts[1] is not PatternPart.Dynamic dynamic)
                throw new Exception($"Second part must be dynamic. Current value: {parts}");

            if (parts[2] is not PatternPart.Static end)
                throw new Exception($"Third part must be static. Current value: {parts}");

            return new TextPatternPart.Dynamic(
                start.Value,
                end.Value,
                CreateMatchFunctionWithSaveVariable(@dynamic.Value, context));
        }

        throw new Exception($"'{parts}' contains more than 3 block static or dynamic");
    }

    private IMatchFunctionTyped CreateMatchFunction(string invoke, ParsingContext context)
    {
        if (invoke.StartsWith(Consts.ControlChars.TemplatePrefix))
        {
            var templateName = invoke.TrimStart(Consts.ControlChars.TemplatePrefix);

            var template = context.Templates.GetOrThrow(templateName);
            return CreateMatchFunction(template.Value, context);
        }

        if (_functionFactorySystem.TryCreateMatchFunction(invoke, out var function))
            return function;

        if (context.CustomDicts.TryGetCustomSetFunction(invoke, out var customSetFunction))
            return customSetFunction;

        var (codeBlock, newRuntimeVariables) = CodeParser.Parse(invoke, context.DeclaredItems);
        context.DeclaredItems.Variables.AddRange(newRuntimeVariables);

        var createFunctionResult = _functionFactoryCSharp.TryCreateMatchFunction(new DeclaredPartsProvider(context.DeclaredItems), codeBlock);
        return createFunctionResult.GetFunctionOrThrow(invoke, context);
    }

    private IMatchFunction CreateMatchFunctionWithSaveVariable(string value, ParsingContext context)
    {
        var (invoke, maybeVariableDeclaration) = value.SplitToTwoPartsFromEnd(Consts.ControlChars.WriteToVariablePrefix).Trim();

        // case: {{ dec >> $$amount }}
        if (maybeVariableDeclaration != null && maybeVariableDeclaration.StartsWith(Consts.ControlChars.VariablePrefix))
        {
            if (maybeVariableDeclaration.Split(" ").Length > 1)
                throw new Exception($"Invalid write to variable declaration: {Consts.ControlChars.WriteToVariablePrefix} " + maybeVariableDeclaration);

            var (name, typeStr) = maybeVariableDeclaration.SplitToTwoParts(Consts.ControlChars.SetType);

            var type = typeStr == null ? null : ReturnType.Parse(typeStr);

            var matchFunction = CreateMatchFunction(invoke, context);

            var variable = new RuntimeVariable(new CustomItemName(name.TrimStart(Consts.ControlChars.VariablePrefix)), type ?? matchFunction.ValueType);
            context.DeclaredItems.Variables.Add(variable);
            return new MatchFunctionWithSaveVariable(matchFunction, variable);
        }

        return CreateMatchFunction(value, context);
    }

    private class RequestMatchersBuilder
    {
        public readonly List<IRequestMatcher> Matchers = new();

        public void AddRange(IEnumerable<IRequestMatcher> matchers)
        {
            foreach (var matcher in matchers)
            {
                Add(matcher);
            }
        }

        public void Add(IRequestMatcher matcher)
        {
            var type = matcher.GetType();
            if (Matchers.FirstOrDefault(x => x.GetType() == matcher.GetType()) != null)
                throw new InvalidOperationException($"Matcher '{type}' already added");

            Matchers.Add(matcher);
        }
    }
}