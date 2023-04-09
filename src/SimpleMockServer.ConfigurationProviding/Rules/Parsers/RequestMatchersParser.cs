using System.Web;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body.Functions;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Headers;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Method;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Path;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.QueryString;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules.Parsers;

class RequestMatchersParser
{
    private readonly FunctionFactory _functionFactory;

    public RequestMatchersParser(FunctionFactory functionFactory)
    {
        _functionFactory = functionFactory;
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

        string methodAndPath = lines[0];

        (string method, string? pathAndQuery) = methodAndPath.SplitToTwoParts(" ").Trim();

        result.Add(CreateMethodRequestMather(method));

        if (pathAndQuery == null)
            return result;
        
        (string path, string? query) = methodAndPath.SplitToTwoParts(" ").Trim();
        
        result.Add(CreatePathRequestMatcher(path));
        
        if(query != null)
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
        if (!Constants.HttpMethods.Contains(method))
            throw new Exception($"String '{method}' not http method");

        return new MethodRequestMatcher(new HttpMethod(method));
    }

    private PathRequestMatcher CreatePathRequestMatcher(string path)
    {
        if (path.Length == 0)
            throw new Exception("An error occurred while creating PathRequestMatcher. Path is empty");

        if (!path.StartsWith('/'))
            throw new Exception($"Matching path must start with '/'. Current value: '{path}'");

        string[] rawSegments = path.Split('/');

        var patterns = CreatePatterns(rawSegments);
        return new PathRequestMatcher(patterns);
    }
    
    private QueryStringRequestMatcher CreateQueryStringMatcher(string queryString)
    {
        var pars = HttpUtility.ParseQueryString(queryString);

        var patterns = new Dictionary<string, ValuePattern>();

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
        var headers = new Dictionary<string, ValuePattern>();

        foreach (var line in block.Lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            (string headerName, string headerPattern) = line.SplitToTwoPartsRequired(Constants.ManageChar.HeaderSplitter);
            
            headers.Add(headerName, CreateValuePattern(headerPattern));
        }

        return new HeadersRequestMatcher(headers);
    }
    
    private IRequestMatcher CreateBodyRequestMatcher(FileBlock block)
    {
        var patterns = new List<KeyValuePair<IExtractFunction, ValuePattern>>();

        foreach (var line in block.Lines)
        {
            if (!line.Contains(Consts.FunctionSplitter))
            {
                patterns.Add(new KeyValuePair<IExtractFunction, ValuePattern>(new AllExtractFunction(), CreateValuePattern(line.Trim())));
                continue;
            }

            (string extractFunctionInvoke, string pattern) = line.SplitToTwoPartsRequired(Constants.ManageChar.Lambda).TrimRequired();
            
            // can write either
            // {{ xpath://employee[1]/text() }}
            // or
            // xpath://employee[1]/text()
            extractFunctionInvoke = extractFunctionInvoke
                .Trim()
                .TrimStart(Consts.ExecutedBlock.Begin)
                .TrimEnd(Consts.ExecutedBlock.End)
                .Trim();

            var extractFunction = _functionFactory.CreateExtractFunction(extractFunctionInvoke);

            patterns.Add(new KeyValuePair<IExtractFunction, ValuePattern>(extractFunction, CreateValuePattern(pattern)));
        }

        return new BodyRequestMatcher(patterns);
    }

    private IReadOnlyList<ValuePattern> CreatePatterns(string[] rawValues)
    {
        var patterns = new List<ValuePattern>(rawValues.Length);

        foreach (var rawValue in rawValues)
        {
            patterns.Add(CreateValuePattern(rawValue));
        }

        return patterns;
    }

    private ValuePattern CreateValuePattern(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return new ValuePattern.NullOrEmpty();

        if (!rawValue.Contains(Consts.ExecutedBlock.Begin) && !rawValue.Contains(Consts.ExecutedBlock.End))
            return new ValuePattern.Static(rawValue);

        if (!rawValue.Contains(Consts.ExecutedBlock.Begin))
            throw new Exception($"Not found begin block for '{rawValue}'");

        if (!rawValue.Contains(Consts.ExecutedBlock.End))
            throw new Exception($"Not found end block for '{rawValue}'");

        string start = rawValue.Substring(0, rawValue.IndexOf(Consts.ExecutedBlock.Begin, StringComparison.Ordinal));
        string end = rawValue.Substring(rawValue.IndexOf(Consts.ExecutedBlock.End, StringComparison.Ordinal) + Consts.ExecutedBlock.End.Length);

        string methodCallString = rawValue
            .TrimStart(start).TrimStart(Consts.ExecutedBlock.Begin)
            .TrimEnd(end).TrimEnd(Consts.ExecutedBlock.End)
            .Trim();

        return new ValuePattern.Dynamic(start, end, _functionFactory.CreateStringMatchFunction(methodCallString));
    }
}
