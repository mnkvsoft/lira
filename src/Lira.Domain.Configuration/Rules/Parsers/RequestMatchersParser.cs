using System.Web;
using Lira.Domain.Matching.Request;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.System;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules.Parsers;

class RequestMatchersParser
{
    private readonly IFunctionFactorySystem _functionFactorySystem;
    private readonly IFunctionFactoryCSharp _functionFactoryCSharp;

    public RequestMatchersParser(IFunctionFactorySystem functionFactorySystem, IFunctionFactoryCSharp functionFactoryCSharp)
    {
        _functionFactorySystem = functionFactorySystem;
        _functionFactoryCSharp = functionFactoryCSharp;
    }

    public RequestMatcherSet Parse(FileSection ruleSection, ParsingContext context)
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

        return (new RequestMatcherSet(
            builder.GetOrNull<MethodRequestMatcher>(),
            builder.GetOrNull<PathRequestMatcher>(),
            builder.GetOrNull<QueryStringRequestMatcher>(),
            builder.GetOrNull<HeadersRequestMatcher>(),
            builder.GetOrNull<BodyRequestMatcher>()));
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
        if (block.Name == Constants.BlockName.Rule.Method)
            return CreateMethodRequestMather(block.GetSingleLine());

        if (block.Name == Constants.BlockName.Rule.Query)
        {
            var query = block.GetSingleLine();
            return CreateQueryStringMatcher(PatternParser.Parse(query), context);
        }

        if (block.Name == Constants.BlockName.Rule.Headers)
            return CreateHeadersRequestMatcher(block, context);

        if (block.Name == Constants.BlockName.Rule.Body)
            return CreateBodyRequestMatcher(block, context);

        throw new Exception($"Unknown block '{block.Name}' in 'rule' section");
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

        for (var i = 0; i < segments.Count; i++)
        {
            patterns.Add(CreateValuePattern(segments[i], context));
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
                if (!_functionFactorySystem.TryCreateBodyExtractFunction(FunctionName.ExtractBody.All, out var function))
                    throw new InvalidOperationException(
                        $"Cannot create system function extract body function '{FunctionName.ExtractBody.All}'");

                patterns.Add(new KeyValuePair<IBodyExtractFunction, TextPatternPart>(function, CreateValuePattern(line, context)));
                continue;
            }

            var (extractFunctionInvoke, pattern) = line.SplitToTwoPartsRequired(Consts.ControlChars.PipelineSplitter).Trim();

            // can write either
            // {{ xpath://employee[1]/text() }}
            // or
            // xpath://employee[1]/text()
            var extractFunctionInvokeStr = extractFunctionInvoke.GetSingleDynamic().Value.Trim();

            if (!_functionFactorySystem.TryCreateBodyExtractFunction(extractFunctionInvokeStr, out var bodyExtractFunction))
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
        if(parts == null || parts.Count == 0)
            return new TextPatternPart.NullOrEmpty(); 
        
        if (parts.Count == 1)
        {
            if (parts[0] is PatternPart.Static stat)
                return new TextPatternPart.Static(stat.Value);

            if (parts[0] is PatternPart.Dynamic dyn)
            {
                var (id, invoke) = GetIdAndRawInvoke(dyn.Value);
                return new TextPatternPart.Dynamic(Start: null, End: null, CreateMatchFunction(invoke, context), id);
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
                var (id, invoke) = GetIdAndRawInvoke(dyn.Value);
                return new TextPatternPart.Dynamic(Start: @static.Value, End: null,
                    CreateMatchFunction(invoke, context), id);
            }

            if (parts[0] is PatternPart.Static dynamic)
            {
                var (id, invoke) = GetIdAndRawInvoke(dynamic.Value);
                return new TextPatternPart.Dynamic(Start: null, End: ((PatternPart.Static)parts[1]).Value,
                    CreateMatchFunction(invoke, context), id);
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

            var (id, invoke) = GetIdAndRawInvoke(@dynamic.Value);
            return new TextPatternPart.Dynamic(start.Value, end.Value, CreateMatchFunction(invoke, context), id);
        }

        throw new Exception($"'{parts}' contains more than 3 block static or dynamic");
    }

    private IMatchFunction CreateMatchFunction(string invoke, ParsingContext context)
    {
        if (invoke.StartsWith(Consts.ControlChars.TemplatePrefix))
        {
            var templateName = invoke.TrimStart(Consts.ControlChars.TemplatePrefix);

            var template = context.Templates.GetOrThrow(templateName);
            return CreateMatchFunction(template.Value, context);
        }

        if (_functionFactorySystem.TryCreateMatchFunction(invoke, out var function))
            return function;

        if (context.CustomSets.TryGetCustomSetFunction(invoke, out var customSetFunction))
            return customSetFunction;

        var createFunctionResult = _functionFactoryCSharp.TryCreateMatchFunction(new DeclaredPartsProvider(context.DeclaredItems), invoke);
        return createFunctionResult.GetFunctionOrThrow(invoke, context);
    }

    private static (string? id, string invoke) GetIdAndRawInvoke(string value)
    {
        if (value.StartsWith(":"))
        {
            var (id, invoke) = value.TrimStart(":").TrimStart().SplitToTwoPartsRequired(" ").Trim();
            return (id, invoke);
        }

        return (null, value);
    }

    private class RequestMatchersBuilder
    {
        private readonly List<IRequestMatcher> _matchers = new();

        public TRequestMatcher? GetOrNull<TRequestMatcher>() where TRequestMatcher : class, IRequestMatcher
        {
            var result = _matchers.FirstOrDefault(m => m is TRequestMatcher);
            return (TRequestMatcher?)result;
        }

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
            if (_matchers.FirstOrDefault(x => x.GetType() == matcher.GetType()) != null)
                throw new InvalidOperationException($"Matcher '{type}' already added");

            _matchers.Add(matcher);
        }
    }
}