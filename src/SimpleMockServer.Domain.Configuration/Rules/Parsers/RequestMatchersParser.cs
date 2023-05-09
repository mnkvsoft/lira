using System.Web;
using SimpleMockServer.Common.Extensions;
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

    public RequestMatchersParser(IStringMatchFunctionFactory stringMatchFunctionFactory, IBodyExtractFunctionFactory bodyExtractFunctionFactory)
    {
        _stringMatchFunctionFactory = stringMatchFunctionFactory;
        _bodyExtractFunctionFactory = bodyExtractFunctionFactory;
    }

    public RequestMatcherSet Parse(FileSection ruleSection)
    {
        var matchers = new RequestMatcherSet();
        matchers.AddRange(GetMethodAndPathMatchersFromShortEntry(ruleSection));
        foreach (var block in ruleSection.Blocks)
        {
            matchers.Add(CreateRequestMatcher(block));
        }

        return matchers;
    }

    private IReadOnlyCollection<IRequestMatcher> GetMethodAndPathMatchersFromShortEntry(FileSection ruleSection)
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

        result.Add(CreatePathRequestMatcher(path));

        if (query != null)
            result.Add(CreateQueryStringMatcher(query));

        return result;
    }

    private IRequestMatcher CreateRequestMatcher(FileBlock block)
    {
        if (block.Name == Constants.BlockName.Rule.Method)
            return CreateMethodRequestMather(block.GetSingleLine());

        if (block.Name == Constants.BlockName.Rule.Path)
            return CreatePathRequestMatcher(block.GetSingleLine());

        if (block.Name == Constants.BlockName.Rule.Query)
            return CreateQueryStringMatcher(block.GetSingleLine());

        if (block.Name == Constants.BlockName.Rule.Headers)
            return CreateHeadersRequestMatcher(block);

        if (block.Name == Constants.BlockName.Rule.Body)
            return CreateBodyRequestMatcher(block);

        throw new Exception($"Unknown block '{block.Name}' in 'rule' section");
    }

    private static MethodRequestMatcher CreateMethodRequestMather(string method)
    {
        return new MethodRequestMatcher(method.ToHttpMethod());
    }

    private PathRequestMatcher CreatePathRequestMatcher(string path)
    {
        if (path.Length == 0)
            throw new Exception("An error occurred while creating PathRequestMatcher. Path is empty");

        if (!path.StartsWith('/'))
            throw new Exception($"Matching path must start with '/'. Current value: '{path}'");

        var rawSegments = path.Split('/');

        var patterns = CreatePatterns(rawSegments);
        return new PathRequestMatcher(patterns);
    }

    private QueryStringRequestMatcher CreateQueryStringMatcher(string queryString)
    {
        var pars = HttpUtility.ParseQueryString(queryString);

        var patterns = new Dictionary<string, TextPatternPart>();

        foreach (var key in pars.AllKeys)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception($"Key is empty in '{queryString}'");

            patterns.Add(key, CreateValuePattern(pars[key]));
        }

        return new QueryStringRequestMatcher(patterns);
    }

    private HeadersRequestMatcher CreateHeadersRequestMatcher(FileBlock block)
    {
        var headers = new Dictionary<string, TextPatternPart>();

        foreach (var line in block.Lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            var (headerName, headerPattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.HeaderSplitter).Trim();

            headers.Add(headerName, CreateValuePattern(headerPattern));
        }

        return new HeadersRequestMatcher(headers);
    }

    private IRequestMatcher CreateBodyRequestMatcher(FileBlock block)
    {
        var patterns = new List<KeyValuePair<IBodyExtractFunction, TextPatternPart>>();

        foreach (var line in block.Lines)
        {
            if (!line.Contains(Consts.FunctionSplitter))
            {
                patterns.Add(new KeyValuePair<IBodyExtractFunction, TextPatternPart>(_bodyExtractFunctionFactory.Create(FunctionName.ExtractBody.All), CreateValuePattern(line.Trim())));
                continue;
            }

            var (extractFunctionInvoke, pattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.Lambda).Trim();

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

            patterns.Add(new KeyValuePair<IBodyExtractFunction, TextPatternPart>(extractFunction, CreateValuePattern(pattern)));
        }

        return new BodyRequestMatcher(patterns);
    }

    private IReadOnlyList<TextPatternPart> CreatePatterns(string[] rawValues)
    {
        var patterns = new List<TextPatternPart>(rawValues.Length);

        foreach (var rawValue in rawValues)
        {
            patterns.Add(CreateValuePattern(rawValue));
        }

        return patterns;
    }

    private TextPatternPart CreateValuePattern(string? rawValue)
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
        var end = rawValue.Substring(rawValue.IndexOf(Consts.ExecutedBlock.End, StringComparison.Ordinal) + Consts.ExecutedBlock.End.Length);

        var methodCallString = rawValue
            .TrimStart(start).TrimStart(Consts.ExecutedBlock.Begin)
            .TrimEnd(end).TrimEnd(Consts.ExecutedBlock.End)
            .Trim();

        return new TextPatternPart.Dynamic(start, end, _stringMatchFunctionFactory.Create(methodCallString));
    }
}
