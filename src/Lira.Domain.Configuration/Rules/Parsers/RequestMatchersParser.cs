﻿using System.Web;
using Lira.Domain.Matching.Request;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Configuration.Templating;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.String;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules.Parsers;

class RequestMatchersParser
{
    private readonly IBodyExtractFunctionFactory _bodyExtractFunctionFactory;
    private readonly IStringMatchFunctionFactory _stringMatchFunctionFactory;

    public RequestMatchersParser(IStringMatchFunctionFactory stringMatchFunctionFactory,
        IBodyExtractFunctionFactory bodyExtractFunctionFactory)
    {
        _stringMatchFunctionFactory = stringMatchFunctionFactory;
        _bodyExtractFunctionFactory = bodyExtractFunctionFactory;
    }


    public (RequestMatcherSet Set, IReadOnlyCollection<PathNameMap> PathNameMaps) Parse(FileSection ruleSection, IReadOnlyCollection<Template> templates)
    {
        var builder = new RequestMatchersBuilder();
        IReadOnlyCollection<PathNameMap> pathNameMaps;

        var (matchers, pathMaps) = GetMethodAndPathMatchersFromShortEntry(ruleSection, templates);
        builder.AddRange(matchers);
        pathNameMaps = pathMaps;
        
        ruleSection.AssertContainsOnlyKnownBlocks(BlockNameHelper.GetBlockNames<Constants.BlockName.Rule>());
        
        foreach (var block in ruleSection.Blocks)
        {
            if (block.Name == Constants.BlockName.Rule.Path)
            {
                var (requestMatcher, nameMaps) = CreatePathRequestMatcher(block.GetSingleLine(), templates);

                if (pathNameMaps.Count != 0 && nameMaps.Count != 0)
                    throw new Exception("Path segment with name map already exist");

                pathNameMaps = nameMaps;
                
                builder.Add(requestMatcher);  
                continue;
            }

            builder.Add(CreateRequestMatcher(block, templates));
        }

        return (new RequestMatcherSet(
            builder.GetOrNull<MethodRequestMatcher>(),
            builder.GetOrNull<PathRequestMatcher>(),
            builder.GetOrNull<QueryStringRequestMatcher>(),
            builder.GetOrNull<HeadersRequestMatcher>(),
            builder.GetOrNull<BodyRequestMatcher>()), pathNameMaps);
    }

    private (IReadOnlyCollection<IRequestMatcher> Matchers, IReadOnlyCollection<PathNameMap> PathNameMaps) GetMethodAndPathMatchersFromShortEntry(FileSection ruleSection,
        IReadOnlyCollection<Template> templates)
    {
        var lines = ruleSection.LinesWithoutBlock;

        if (lines.Count == 0)
            return (Array.Empty<IRequestMatcher>(), Array.Empty<PathNameMap>());

        if (lines.Count > 1)
            throw new Exception("Rule section contains several sections: " + string.Join(", ", lines.Select(s => $"'{s}'")));

        var result = new List<IRequestMatcher>();

        var methodAndPath = lines[0];

        var (method, pathAndQuery) = methodAndPath.SplitToTwoParts(" ").Trim();

        result.Add(CreateMethodRequestMather(method));

        if (pathAndQuery == null)
            return (result, Array.Empty<PathNameMap>());

        var (path, query) = pathAndQuery.SplitToTwoParts("?").Trim();

        var (pathMatcher, pathNameMaps) = CreatePathRequestMatcher(path, templates); 
        result.Add(pathMatcher);

        if (query != null)
            result.Add(CreateQueryStringMatcher(query, templates));

        return (result, pathNameMaps);
    }

    private IRequestMatcher CreateRequestMatcher(FileBlock block, IReadOnlyCollection<Template> templates)
    {
        if (block.Name == Constants.BlockName.Rule.Method)
            return CreateMethodRequestMather(block.GetSingleLine());

        if (block.Name == Constants.BlockName.Rule.Query)
            return CreateQueryStringMatcher(block.GetSingleLine(), templates);

        if (block.Name == Constants.BlockName.Rule.Headers)
            return CreateHeadersRequestMatcher(block, templates);

        if (block.Name == Constants.BlockName.Rule.Body)
            return CreateBodyRequestMatcher(block, templates);

        throw new Exception($"Unknown block '{block.Name}' in 'rule' section");
    }

    private static MethodRequestMatcher CreateMethodRequestMather(string method)
    {
        return new MethodRequestMatcher(method.ToHttpMethod());
    }

    private (PathRequestMatcher Matcher, IReadOnlyCollection<PathNameMap> PathNameMaps) CreatePathRequestMatcher(string path, IReadOnlyCollection<Template> templates)
    {
        if (path.Length == 0)
            throw new Exception("An error occurred while creating PathRequestMatcher. Path is empty");

        if (!path.StartsWith('/'))
            throw new Exception($"Matching path must start with '/'. Current value: '{path}'");
        
        List<string> rawSegments = GetRawSegments(path);

        var patterns = new List<TextPatternPart>(rawSegments.Count);

        var maps = new List<PathNameMap>();
        for (var i = 0; i < rawSegments.Count; i++)
        {
            var rawValue = rawSegments[i];
            string? segmentName = null;

            string ExtractName(string rawInvoke)
            {
                var (invoke, name) = rawInvoke.SplitToTwoParts("name:").Trim();
                segmentName = name;
                return invoke;
            }

            patterns.Add(CreateValuePattern(rawValue, templates, ExtractName));
            
            if(!string.IsNullOrWhiteSpace(segmentName))
                maps.Add(new PathNameMap(Index: i, segmentName));
        }

        return (new PathRequestMatcher(patterns), maps);
    }

    // because dynamic blocks can have a symbol / then parse like this
    private static List<string> GetRawSegments(string path)
    {
        var parsed = PatternParser.Parse(path);
        var rawSegments = new List<string>(15);

        string? remainder = null;
        foreach (PatternPart patternPart in parsed)
        {
            if (patternPart is PatternPart.Static @static)
            {
                var segmentsTemp = @static.Value.Split('/');
                for (int i = 0; i < segmentsTemp.Length; i++)
                {
                    string s = segmentsTemp[i];
                    
                    if (i == 0 && remainder != null)
                    {
                        rawSegments.Add(remainder + s);
                        remainder = null;
                        continue;
                    }
                    
                    if (i == segmentsTemp.Length - 1)
                    {
                        remainder = s;
                        break;
                    }

                    rawSegments.Add(s);
                }
            }
            else if (patternPart is PatternPart.Dynamic dynamic)
            {
                string value = Consts.ExecutedBlock.Begin + dynamic.Value + Consts.ExecutedBlock.End;
                if (remainder != null)
                    remainder += value;
                else
                    remainder = value;
            }
            else
            {
                throw new UnsupportedInstanceType(patternPart);
            }
        }

        if (remainder != null)
            rawSegments.Add(remainder);
        
        return rawSegments;
    }

    private QueryStringRequestMatcher CreateQueryStringMatcher(string queryString, IReadOnlyCollection<Template> templates)
    {
        var pars = HttpUtility.ParseQueryString(queryString);

        var patterns = new Dictionary<string, TextPatternPart>();

        foreach (var key in pars.AllKeys)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception($"Key is empty in '{queryString}'");

            patterns.Add(key, CreateValuePattern(pars[key], templates));
        }

        return new QueryStringRequestMatcher(patterns);
    }

    private HeadersRequestMatcher CreateHeadersRequestMatcher(FileBlock block, IReadOnlyCollection<Template> templates)
    {
        var headers = new Dictionary<string, TextPatternPart>();

        foreach (var line in block.Lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            var (headerName, headerPattern) = line.SplitToTwoPartsRequired(Consts.ControlChars.HeaderSplitter).Trim();

            headers.Add(headerName, CreateValuePattern(headerPattern, templates));
        }

        return new HeadersRequestMatcher(headers);
    }

    private IRequestMatcher CreateBodyRequestMatcher(FileBlock block, IReadOnlyCollection<Template> templates)
    {
        var patterns = new List<KeyValuePair<IBodyExtractFunction, TextPatternPart>>();

        foreach (var line in block.Lines)
        {
            if (!line.Contains(Consts.ControlChars.PipelineSplitter))
            {
                patterns.Add(new KeyValuePair<IBodyExtractFunction, TextPatternPart>(
                    _bodyExtractFunctionFactory.Create(FunctionName.ExtractBody.All), CreateValuePattern(line.Trim(), templates)));
                continue;
            }

            var (extractFunctionInvoke, pattern) = line.SplitToTwoPartsRequired(Consts.ControlChars.PipelineSplitter).Trim();

            // can write either
            // {{ xpath://employee[1]/text() }}
            // or
            // xpath://employee[1]/text()
            extractFunctionInvoke = extractFunctionInvoke
                .Trim()
                .TrimStart(Consts.ExecutedBlock.Begin)
                .TrimEnd(Consts.ExecutedBlock.End)
                .Trim();

            var extractFunction = _bodyExtractFunctionFactory.Create(extractFunctionInvoke);

            patterns.Add(new KeyValuePair<IBodyExtractFunction, TextPatternPart>(extractFunction, CreateValuePattern(pattern, templates)));
        }

        return new BodyRequestMatcher(patterns);
    }

    private TextPatternPart CreateValuePattern(string? rawValue, IReadOnlyCollection<Template> templates, Func<string, string>? extractCustomValueFromDynamic = null)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return new TextPatternPart.NullOrEmpty();

        if (!rawValue.Contains(Consts.ExecutedBlock.Begin) && !rawValue.Contains(Consts.ExecutedBlock.End))
            return new TextPatternPart.Static(rawValue);

        if (!rawValue.Contains(Consts.ExecutedBlock.Begin))
            throw new Exception($"Not found begin block for '{rawValue}'");

        if (!rawValue.Contains(Consts.ExecutedBlock.End))
            throw new Exception($"Not found end block for '{rawValue}'");

        var start = rawValue.Substring(0, rawValue.IndexOf(Consts.ExecutedBlock.Begin, StringComparison.Ordinal));
        var end = rawValue.Substring(rawValue.IndexOf(Consts.ExecutedBlock.End, StringComparison.Ordinal) +
                                     Consts.ExecutedBlock.End.Length);

        var invoke = rawValue
            .TrimStart(start).TrimStart(Consts.ExecutedBlock.Begin)
            .TrimEnd(end).TrimEnd(Consts.ExecutedBlock.End)
            .Trim();

        if (extractCustomValueFromDynamic != null)
            invoke = extractCustomValueFromDynamic(invoke);
        
        if (invoke.StartsWith(Consts.ControlChars.TemplatePrefix))
        {
            var templateName = invoke.TrimStart(Consts.ControlChars.TemplatePrefix);

            var template = templates.GetOrThrow(templateName);
            return CreateValuePattern(template.Value, templates, extractCustomValueFromDynamic);
        }

        return new TextPatternPart.Dynamic(start, end, _stringMatchFunctionFactory.Create(invoke));
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