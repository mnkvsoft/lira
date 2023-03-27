using System.Reflection;
using System.Text;
using System.Web;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Generating.Writers;
using SimpleMockServer.Domain.Models.RulesModel.Matching;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body.Functions;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Headers;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Method;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Path;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.QueryString;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules;

class RulesFileParser
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly FunctionFactory _functionFactory;

    public RulesFileParser(ILoggerFactory loggerFactory, FunctionFactory functionFactory)
    {
        _loggerFactory = loggerFactory;
        _functionFactory = functionFactory;
    }

    static class CommentChar
    {
        public const string SingleLine = "//";
        public const string MultiLineStart = "/*";
        public const string MultiLineEnd = "*/";
    }

    static class SectionName
    {
        public const string Rule = "rule";
        public const string Condition = "condition";
        public const string Response = "response";
        public const string Callback = "callback";
    }

    static class BlockName
    {
        public class Rule
        {
            public const string Method = "method";
            public const string Path = "path";
            public const string Query = "query";
            public const string Headers = "headers";
            public const string Body = "body";
        }

        public class Response
        {
            public const string Code = "code";
            public const string Headers = "headers";
            public const string Body = "body";
        }
    }

    private static IReadOnlySet<string> GetBlockNames<T>()
    {
        var set = new HashSet<string>();
        var values = GetAllPublicConstantValues<string>(typeof(T));

        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception("Empty block name");

            set.AddOrThrowIfContains(value);
        }

        return set;
    }

    private static IReadOnlyCollection<T?> GetAllPublicConstantValues<T>(Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
            .Select(x => (T?)x.GetRawConstantValue())
            .ToList();
    }

    public async Task<IReadOnlyCollection<RuleWithExtInfo>> Parse(string ruleFile)
    {
        IReadOnlyCollection<Rule> rules = await GetRules(ruleFile);

        var result = new List<RuleWithExtInfo>();
        foreach (var rule in rules)
        {
            result.Add(new RuleWithExtInfo(
                rule,
                ruleFile));
        }
        return result;
    }

    private async Task<IReadOnlyCollection<Rule>> GetRules(string ruleFile)
    {
        try
        {
            var rulesSections = await SectionFileParser.Parse(
            ruleFile,
            knownBlockForSections: new Dictionary<string, IReadOnlySet<string>>
            {
                {"rule", GetBlockNames<BlockName.Rule>() },
                {"response", GetBlockNames<BlockName.Response>() },
            },
            maxNestingDepth: 3);

            AssertContainsOnlySections(rulesSections, SectionName.Rule);

            var rules = new List<Rule>();
            for (int i = 0; i < rulesSections.Count; i++)
            {
                FileSection? ruleSection = rulesSections[i];
                var fi = new FileInfo(ruleFile);
                var ruleName = $"no. {i + 1} file: {fi.Name}";
                rules.AddRange(CreateRules(ruleName, ruleSection));
            }

            return rules;
        }
        catch (Exception exc)
        {
            throw new Exception("Error occured on parsing file: " + ruleFile, exc);
        }
    }

    private static void AssertContainsOnlySections(IReadOnlyList<FileSection> rulesSections, params string[] expectedSectionName)
    {
        var unknownSections = rulesSections
            .Where(s => !expectedSectionName.Contains(s.Name))
            .Select(x => x.Name)
            .ToArray();

        if (unknownSections.Length != 0)
            throw new Exception("Unknown sections: " + string.Join(", ", unknownSections) + ". Expected: " + string.Join(", ", expectedSectionName));
    }

    private IReadOnlyCollection<Rule> CreateRules(string ruleName, FileSection ruleSection)
    {
        var childSections = ruleSection.ChildSections;

        if (childSections.Count == 0)
            throw new Exception("Rule section is empty");

        string? alias = GetAlias(ruleSection);

        var matcherSet = CreateMatchers(ruleSection);

        var section = childSections[0];

        if (section.Name == SectionName.Condition)
        {
            AssertContainsOnlySections(childSections, SectionName.Condition);

            throw new NotImplementedException();
        }
        else
        {
            AssertContainsOnlySections(childSections, SectionName.Response, SectionName.Callback);

            ResponseWriter responseWriter = CreateResponseWriter(ruleSection);

            return new Rule[] { new Rule(ruleName, _loggerFactory, responseWriter, matcherSet) };
        }
    }

    private static string? GetAlias(FileSection ruleSection)
    {
        string? alias = null;
        if (ruleSection.Name.Contains(' '))
        {
            string name = ruleSection.Name;
            alias = name.Substring(name.IndexOf(' '));
        }

        return alias;
    }

    private ResponseWriter CreateResponseWriter(FileSection ruleSection)
    {
        var responseSection = ruleSection.GetSingleChildSection(SectionName.Response);

        int httpCode = GetHttpCode(responseSection);
        HeadersWriter? headersWriter = GetHeadersWriter(responseSection);
        BodyWriter? bodyWriter = GetBodyWriter(responseSection);

        var responseWriter = new ResponseWriter(httpCode, bodyWriter, headersWriter);
        return responseWriter;
    }

    private static int GetHttpCode(FileSection responseSection)
    {
        int httpCode;

        if (responseSection.LinesWithoutBlock.Count > 0)
        {
            httpCode = ParseHttpCode(responseSection.GetSingleLine());
        }
        else
        {
            var codeBlock = responseSection.GetBlockRequired(BlockName.Response.Code);
            httpCode = ParseHttpCode(codeBlock.GetSingleLine());
        }

        return httpCode;
    }

    private BodyWriter? GetBodyWriter(FileSection responseSection)
    {
        BodyWriter? bodyWriter = null;
        var bodyBlock = responseSection.GetBlockOrNull(BlockName.Response.Body);

        if (bodyBlock != null)
        {
            var parts = CreateValueParts(bodyBlock.GetStringValue());
            bodyWriter = new BodyWriter(new ValuePartSet(parts));
        }

        return bodyWriter;
    }

    private HeadersWriter? GetHeadersWriter(FileSection responseSection)
    {
        HeadersWriter? headersWriter = null;

        var headersBlock = responseSection.GetBlockOrNull(BlockName.Response.Headers);
        if (headersBlock != null)
        {
            var headers = new Dictionary<string, ValuePartSet>();
            foreach (var line in headersBlock.Lines)
            {
                if (string.IsNullOrEmpty(line))
                    break;

                (string headerName, string? headerPattern) = line.SplitToTwoParts(":").Trim();

                if (headerPattern == null)
                    throw new Exception($"Empty matching for header '{headerPattern}'");

                var parts = CreateValueParts(headerPattern);

                headers.Add(headerName, new ValuePartSet(parts));

            }
            headersWriter = new HeadersWriter(headers);
        }

        return headersWriter;
    }

    private IReadOnlyCollection<ValuePart> CreateValueParts(string pattern)
    {
        var patternParts = PatternParser.Parse(pattern);

        var valueParts = new List<ValuePart>();
        foreach (var patternPart in patternParts)
        {
            valueParts.Add(CreateValuePart(patternPart));
        }

        return valueParts;
    }

    private ValuePart CreateValuePart(PatternPart patternPart)
    {
        switch (patternPart)
        {
            case PatternPart.Static:
                return new ValuePart.Static(patternPart.Value);
            case PatternPart.Dynamic dynamicPart:
                return new ValuePart.Dynamic(_functionFactory.CreateGeneratingFunction(dynamicPart.Value));
            default:
                throw new UnsupportedInstanceType(patternPart);
        }
    }

    private static int ParseHttpCode(string str)
    {
        if (!int.TryParse(str, out int httpCode))
            throw new Exception($"Invalid http code: '{str}'");
        return httpCode;
    }

    private RequestMatcherSet CreateMatchers(FileSection ruleSection)
    {
        var matchers = new RequestMatcherSet();
        matchers.AddRange(GetMethodAndPathMatchersFromShortEntry(ruleSection));
        foreach (var block in ruleSection.Blocks)
        {
            matchers.Add(CreateRequestMatcher(block));
        }
        return matchers;
    }

    private IRequestMatcher CreateRequestMatcher(FileBlock block)
    {
        if (block.Name == BlockName.Rule.Method)
        {
            return CreateMethodRequestMather(block.GetSingleLine());
        }
        else if (block.Name == BlockName.Rule.Path)
        {
            return CreatePathRequestMatcher(block.GetSingleLine());
        }
        else if (block.Name == BlockName.Rule.Query)
        {
            return CreateQueryStringMatcher(block.GetSingleLine());
        }
        else if (block.Name == BlockName.Rule.Headers)
        {
            return CreateHeadersRequestMatcher(block);
        }
        else if (block.Name == BlockName.Rule.Body)
        {
            return CreateBodyRequestMatcher(block);
        }
        else
        {
            throw new Exception($"Unknown block '{block.Name}' in 'rule' section");
        }
    }


    public IRequestMatcher CreateBodyRequestMatcher(FileBlock block)
    {
        var patterns = new List<KeyValuePair<IExtractFunction, ValuePattern>>();

        foreach (var line in block.Lines)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            if (!line.Contains(Consts.FunctionSplitter))
            {
                patterns.Add(new KeyValuePair<IExtractFunction, ValuePattern>(new AllExtractFunction(), CreateValuePattern(line.Trim())));
                continue;
            }

            var keyValue = line.Split("=>");
            if (keyValue.Length < 2)
                throw new Exception($"Invalid body line: '{line}'");

            // can write either
            // {{ xpath://employee[1]/text() }}
            // or
            // xpath://employee[1]/text()
            string extractFunctionInvoke = keyValue[0]
                                                 .Trim()
                                                    .TrimStart(Consts.ExecutedBlock.Begin)
                                                    .TrimEnd(Consts.ExecutedBlock.End)
                                                    .Trim();

            var extractFunction = _functionFactory.CreateExtractFunction(extractFunctionInvoke);

            string pattern = keyValue[1].Trim();

            patterns.Add(new KeyValuePair<IExtractFunction, ValuePattern>(extractFunction, CreateValuePattern(pattern)));
        }

        return new BodyRequestMatcher(patterns);
    }



    public HeadersRequestMatcher CreateHeadersRequestMatcher(FileBlock block)
    {
        var headers = new Dictionary<string, ValuePattern>();

        foreach (var line in block.Lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            var keyValue = line.Split(':');
            if (keyValue.Length < 2)
                throw new Exception($"Invalid header line: '{line}'");

            string headerName = keyValue[0].Trim();
            string headerPattern = keyValue[1].Trim();

            headers.Add(headerName, CreateValuePattern(headerPattern));

        }

        return new HeadersRequestMatcher(headers);
    }

    private IReadOnlyCollection<IRequestMatcher> GetMethodAndPathMatchersFromShortEntry(FileSection ruleSection)
    {
        List<IRequestMatcher> result = new List<IRequestMatcher>();
        var lines = ruleSection.LinesWithoutBlock;

        if (lines.Count > 1)
            throw new Exception("Rule section contains severals sectins: " + string.Join(", ", lines.Select(s => $"'{s}'")));

        if (lines.Count == 1)
        {
            string methodAndPath = lines[0];

            int idx = methodAndPath.IndexOf(" ");

            string method = methodAndPath.Substring(0, idx);
            string pathAndQuery = methodAndPath.Substring(idx + 1);

            result.Add(CreateMethodRequestMather(method));

            if (!pathAndQuery.Contains('?'))
            {
                result.Add(CreatePathRequestMatcher(pathAndQuery));
            }
            else
            {
                string[] pathAndQuerySplitted = pathAndQuery.Split('?');

                if (pathAndQuerySplitted.Length > 2)
                    throw new Exception("Path string contains several '?' char");

                result.Add(CreatePathRequestMatcher(pathAndQuerySplitted[0]));
                result.Add(CreateQueryStringMatcher(pathAndQuerySplitted[1]));
            }
        }

        return result;
    }

    private static MethodRequestMatcher CreateMethodRequestMather(string method)
    {
        if (!httpMethods.Contains(method))
            throw new Exception($"String '{method}' not http method");

        return new MethodRequestMatcher(new HttpMethod(method));
    }

    public QueryStringRequestMatcher CreateQueryStringMatcher(string queryString)
    {
        System.Collections.Specialized.NameValueCollection pars = HttpUtility.ParseQueryString(queryString);

        var patterns = new Dictionary<string, ValuePattern>();

        foreach (var key in pars.AllKeys)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception($"Key is empty in '{queryString}'");

            patterns.Add(key, CreateValuePattern(pars[key]));
        }

        return new QueryStringRequestMatcher(patterns);
    }
    public PathRequestMatcher CreatePathRequestMatcher(string path)
    {
        if (path.Length == 0)
            throw new Exception("An error occurred while creating PathRequestMatcher. Path is empty");

        if (!path.StartsWith('/'))
            throw new Exception($"Matching path must start with '/'. Current value: '{path}'");

        string[] rawSegments = path.Split('/');

        var patterns = CreatePatterns(rawSegments);
        return new PathRequestMatcher(patterns);
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

        string start = rawValue.Substring(0, rawValue.IndexOf(Consts.ExecutedBlock.Begin));
        string end = rawValue.Substring(rawValue.IndexOf(Consts.ExecutedBlock.End) + Consts.ExecutedBlock.End.Length);

        string methodCallString = rawValue
            .TrimStart(start).TrimStart(Consts.ExecutedBlock.Begin)
            .TrimEnd(end).TrimEnd(Consts.ExecutedBlock.End)
            .Trim();

        return new ValuePattern.Dynamic(start, end, _functionFactory.CreateMatchFunction(methodCallString));
    }







    private static readonly HashSet<string> httpMethods = new HashSet<string> { "GET", "PUT", "POST", "DELETE", "HEAD", "TRACE", "OPTIONS", "CONNECT" };

    //private async Task<List<RuleBuilder>> GetBuilders(string ruleFile)
    //{
    //    var iterator = new LinesIterator(await File.ReadAllLinesAsync(ruleFile));
    //    List<RuleBuilder> builders = new List<RuleBuilder>();
    //    RuleBuilder? currentBuilder = null;

    //    bool firstSectionFind = false;
    //    FileSection? currentSection = null;
    //    bool isMultiLineComment = false;

    //    while (iterator.Next())
    //    {
    //        var line = iterator.CurrentLine;

    //        string cleanLine = line.Trim();
    //        if (cleanLine.StartsWith(CommentChar.SingleLine) || string.IsNullOrWhiteSpace(cleanLine))
    //            continue;

    //        if (cleanLine.StartsWith(CommentChar.MultiLineStart))
    //        {
    //            isMultiLineComment = true;
    //            continue;
    //        }

    //        if (cleanLine.StartsWith(CommentChar.MultiLineEnd))
    //        {
    //            isMultiLineComment = false;
    //            continue;
    //        }

    //        if (isMultiLineComment)
    //            continue;

    //        if (!firstSectionFind && !IsSection(cleanLine, FileSection.Match))
    //            throw new Exception("First section must be '----- match'");

    //        firstSectionFind = true;

    //        if (IsSection(cleanLine, FileSection.Match))
    //        {
    //            if (currentBuilder != null)
    //            {
    //                builders.Add(currentBuilder!);
    //            }

    //            currentBuilder = new RuleBuilder();
    //            currentBuilder.Alias = GetAlias(cleanLine);
    //            currentSection = FileSection.Match;
    //            continue;
    //        }

    //        if (IsSection(cleanLine, FileSection.Response))
    //        {
    //            currentSection = FileSection.Response;
    //            continue;
    //        }

    //        if (currentSection == FileSection.Match)
    //        {
    //            AddMessageToException(
    //                () => currentBuilder!.RequestMatchers.Add(_requestMatcherFactory.CreateMatcher(GetKeyValue(line), iterator)),
    //                $"An error occurred while reading '{SectionName.Match}' section");
    //            continue;
    //        }

    //        if (currentSection == FileSection.Response)
    //        {
    //            AddMessageToException(
    //               () => currentBuilder!.ResponseFillers.Add(_responseFillerFactory.CreateFiller(GetKeyValue(line), iterator)),
    //               $"An error occurred while reading '{SectionName.Response}' section");
    //            continue;
    //        }
    //    }

    //    if (currentBuilder != null)
    //        builders.Add(currentBuilder!);

    //    return builders;
    //}

    private string GetKeyValue(string line) => line.Replace(":", "");

    private void AddMessageToException(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            throw new Exception(message, e);
        }
    }

    //private string? GetAlias(string cleanLine)
    //{
    //    var splitted = cleanLine.Split(SectionName.Match);
    //    if (splitted.Length > 1 && !string.IsNullOrWhiteSpace(splitted[1]))
    //    {
    //        return splitted[1].Trim();
    //    }
    //    return null;
    //}


    //private bool IsSection(string cleanLine, FileSection fileSection)
    //{
    //    string section = fileSection switch
    //    {
    //        FileSection.Match => SectionName.Match,
    //        FileSection.Response => SectionName.Response,
    //        _ => throw new Exception("Unknown enum: " + fileSection)
    //    };

    //    return cleanLine.Replace(" ", "").StartsWith($"-----{section}", StringComparison.OrdinalIgnoreCase);
    //}


}