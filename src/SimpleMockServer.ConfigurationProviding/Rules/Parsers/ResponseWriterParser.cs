using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Generating.Writers;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules.Parsers;

class ResponseWriterParser
{
    private readonly FunctionFactory _functionFactory;

    public ResponseWriterParser(FunctionFactory functionFactory)
    {
        _functionFactory = functionFactory;
    }

    public ResponseWriter Parse(FileSection ruleSection)
    {
        var responseSection = ruleSection.GetSingleChildSection(Constants.SectionName.Response);

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
            var codeBlock = responseSection.GetBlockRequired(Constants.BlockName.Response.Code);
            httpCode = ParseHttpCode(codeBlock.GetSingleLine());
        }

        return httpCode;
    }

    private BodyWriter? GetBodyWriter(FileSection responseSection)
    {
        BodyWriter? bodyWriter = null;
        var bodyBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Body);

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

        var headersBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Headers);
        if (headersBlock != null)
        {
            var headers = new Dictionary<string, ValuePartSet>();
            foreach (var line in headersBlock.Lines)
            {
                if (string.IsNullOrEmpty(line))
                    break;

                (string headerName, string? headerPattern) = line.SplitToTwoParts(":").Trim();

                if (headerPattern == null)
                    throw new Exception($"Empty matching for header '{headerPattern}' in line: '{line}'");

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
                return new ValuePart.Function(_functionFactory.CreateGeneratingFunction(dynamicPart.Value));
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
}
