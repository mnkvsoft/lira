using System.Web;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Templating;
using SimpleMockServer.Domain.Matching.Request;
using SimpleMockServer.Domain.Matching.Request.Matchers.Body;
using SimpleMockServer.Domain.Matching.Request.Matchers.Headers;
using SimpleMockServer.Domain.Matching.Request.Matchers.Method;
using SimpleMockServer.Domain.Matching.Request.Matchers.Path;
using SimpleMockServer.Domain.Matching.Request.Matchers.QueryString;
using SimpleMockServer.Domain.TextPart.Functions;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Matching.String;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers;

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


    public RequestMatcherSet Parse(FileSection ruleSection, IReadOnlyCollection<Template> templates)
    {
        var builder = new RequestMatchersBuilder();
        builder.AddRange(GetMethodAndPathMatchersFromShortEntry(ruleSection, templates));
        foreach (var block in ruleSection.Blocks)
        {
            builder.Add(CreateRequestMatcher(block, templates));
        }

        return new RequestMatcherSet(
            builder.GetOrNull<MethodRequestMatcher>(),
            builder.GetOrNull<PathRequestMatcher>(),
            builder.GetOrNull<QueryStringRequestMatcher>(),
            builder.GetOrNull<HeadersRequestMatcher>(),
            builder.GetOrNull<BodyRequestMatcher>());
    }

    private IReadOnlyCollection<IRequestMatcher> GetMethodAndPathMatchersFromShortEntry(FileSection ruleSection,
        IReadOnlyCollection<Template> templates)
    {
        var lines = ruleSection.LinesWithoutBlock;

        if (lines.Count == 0)
            return Array.Empty<IRequestMatcher>();

        if (lines.Count > 1)
            throw new Exception("Rule section contains several sections: " + string.Join(", ", lines.Select(s => $"'{s}'")));

        var result = new List<IRequestMatcher>();

        var methodAndPath = lines[0];

        var (method, pathAndQuery) = methodAndPath.SplitToTwoParts(" ").Trim();

        result.Add(CreateMethodRequestMather(method));

        if (pathAndQuery == null)
            return result;

        var (path, query) = pathAndQuery.SplitToTwoParts("?").Trim();

        result.Add(CreatePathRequestMatcher(path, templates));

        if (query != null)
            result.Add(CreateQueryStringMatcher(query, templates));

        return result;
    }

    private IRequestMatcher CreateRequestMatcher(FileBlock block, IReadOnlyCollection<Template> templates)
    {
        if (block.Name == Constants.BlockName.Rule.Method)
            return CreateMethodRequestMather(block.GetSingleLine());

        if (block.Name == Constants.BlockName.Rule.Path)
            return CreatePathRequestMatcher(block.GetSingleLine(), templates);

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

    private PathRequestMatcher CreatePathRequestMatcher(string path, IReadOnlyCollection<Template> templates)
    {
        if (path.Length == 0)
            throw new Exception("An error occurred while creating PathRequestMatcher. Path is empty");

        if (!path.StartsWith('/'))
            throw new Exception($"Matching path must start with '/'. Current value: '{path}'");

        var rawSegments = path.Split('/');

        var patterns = CreatePatterns(rawSegments, templates);
        return new PathRequestMatcher(patterns);
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

    private IReadOnlyList<TextPatternPart> CreatePatterns(string[] rawValues, IReadOnlyCollection<Template> templates)
    {
        var patterns = new List<TextPatternPart>(rawValues.Length);

        foreach (var rawValue in rawValues)
        {
            patterns.Add(CreateValuePattern(rawValue, templates));
        }

        return patterns;
    }

    private TextPatternPart CreateValuePattern(string? rawValue, IReadOnlyCollection<Template> templates)
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

        if (invoke.StartsWith(Consts.ControlChars.TemplatePrefix))
        {
            var templateName = invoke.TrimStart(Consts.ControlChars.TemplatePrefix);

            var template = templates.GetOrThrow(templateName);
            return CreateValuePattern(template.Value, templates);
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
