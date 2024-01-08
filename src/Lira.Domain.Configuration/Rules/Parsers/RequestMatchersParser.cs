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

    public (RequestMatcherSet Set, IReadOnlyCollection<PathNameMap> PathNameMaps) Parse(FileSection ruleSection, ParsingContext context)
    {
        var builder = new RequestMatchersBuilder();

        var (matchers, pathMaps) = GetMethodAndPathMatchersFromShortEntry(ruleSection, context);
        builder.AddRange(matchers);
        IReadOnlyCollection<PathNameMap> pathNameMaps = pathMaps;

        ruleSection.AssertContainsOnlyKnownBlocks(BlockNameHelper.GetBlockNames<Constants.BlockName.Rule>());

        foreach (var block in ruleSection.Blocks)
        {
            if (block.Name == Constants.BlockName.Rule.Path)
            {
                var path = block.GetSingleLine();
                var (requestMatcher, nameMaps) = CreatePathRequestMatcher(PatternParser.Parse(path), context);

                if (pathNameMaps.Count != 0 && nameMaps.Count != 0)
                    throw new Exception("Path segment with name map already exist");

                pathNameMaps = nameMaps;

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
            builder.GetOrNull<BodyRequestMatcher>()), 
            pathNameMaps);
    }

    // (PatternParts Method, PatternParts Path, PatternParts? Query) GetMethodAndPathAndQuery(IReadOnlyCollection<string> lines)
    // {
    //     if (lines.Count == 0)
    //         throw new Exception("Lines is empty");
    //
    //     var allPatterns = PatternParser.Parse(lines);
    //     
    //     var (method, startPath) = allPatterns.SplitToTwoPartsRequired(' ');
    //     
    //     var (path, query) = startPath.SplitToTwoParts('?');
    //
    //     return (method, path, query);
    // }

    private (IReadOnlyCollection<IRequestMatcher> Matchers, IReadOnlyCollection<PathNameMap> PathNameMaps)
        GetMethodAndPathMatchersFromShortEntry(
            FileSection ruleSection,
            ParsingContext context)
    {
        var lines = ruleSection.LinesWithoutBlock;

        if (lines.Count == 0)
            return (Array.Empty<IRequestMatcher>(), Array.Empty<PathNameMap>());

        var result = new List<IRequestMatcher>();

        var allPatterns = PatternParser.Parse(lines);

        var (method, startPath) = allPatterns.SplitToTwoPartsRequired(" ");

        var (path, query) = startPath.SplitToTwoParts("?");

        result.Add(CreateMethodRequestMather(method.SingleStaticValueToString()));

        var (pathMatcher, pathNameMaps) = CreatePathRequestMatcher(path, context);
        result.Add(pathMatcher);

        if (query != null)
            result.Add(CreateQueryStringMatcher(query, context));

        return (result, pathNameMaps);
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

    private (PathRequestMatcher Matcher, IReadOnlyCollection<PathNameMap> PathNameMaps) CreatePathRequestMatcher(PatternParts pathParts,
        ParsingContext context)
    {
        if (pathParts.Count == 0)
            throw new Exception("An error occurred while creating PathRequestMatcher. Path is empty");

        if (pathParts.First() is not PatternPart.Static @static || !@static.Value.StartsWith('/'))
            throw new Exception($"Matching path must start with '/'. Current value: '{pathParts}'");

        var segments = pathParts.Split("/");

        var patterns = new List<TextPatternPart>(segments.Count);

        var maps = new List<PathNameMap>();
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            string? segmentName = null;

            if (segment.ContainsDynamic())
            {
                var dynamic = segment.GetSingleDynamic();

                string invoke;
                string value = dynamic.Value;
                if (value.StartsWith("name") && value.TrimStart("name").TrimStart().StartsWith("="))
                {
                    (segmentName, invoke) = value.TrimStart("name").TrimStart().TrimStart("=").SplitToTwoPartsRequired(" ").Trim();
                }
                else
                {
                    invoke = value;
                }
                
                segment = segment.Replace(dynamic, new PatternPart.Dynamic(invoke));
            }
            
            patterns.Add(CreateValuePattern(segment, context));

            if (!string.IsNullOrWhiteSpace(segmentName))
                maps.Add(new PathNameMap(Index: i, segmentName));
        }

        return (new PathRequestMatcher(patterns), maps);
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
            // if (string.IsNullOrEmpty(line))
            //     break;

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
                return new TextPatternPart.Dynamic(Start: null, End: null, CreateMatchFunction(dyn.Value, context));
        }

        if (parts.Count == 2)
        {
            if (!parts.Any(x => x is PatternPart.Static))
                throw new Exception($"'{parts}' must contains one static part");

            if (!parts.Any(x => x is PatternPart.Static))
                throw new Exception($"'{parts}' must contains one dynamic part");

            if (parts[0] is PatternPart.Static @static)
            {
                return new TextPatternPart.Dynamic(Start: @static.Value, End: null,
                    CreateMatchFunction(((PatternPart.Dynamic)parts[1]).Value, context));
            }

            if (parts[0] is PatternPart.Static dynamic)
            {
                return new TextPatternPart.Dynamic(Start: null, End: ((PatternPart.Static)parts[1]).Value,
                    CreateMatchFunction(dynamic.Value, context));
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

            return new TextPatternPart.Dynamic(start.Value, end.Value, CreateMatchFunction(@dynamic.Value, context));
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

        var createFunctionResult = _functionFactoryCSharp.TryCreateMatchFunction(new DeclaredPartsProvider(context.DeclaredItems), invoke);
        return createFunctionResult.GetFunctionOrThrow(invoke);
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