using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Generating.Writers;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules.Parsers;

class ResponseWriterParser
{
    private readonly ValuePartsCreator _valuePartsCreator;

    public ResponseWriterParser(ValuePartsCreator valuePartsCreator)
    {
        _valuePartsCreator = valuePartsCreator;
    }

    public ResponseWriter Parse(FileSection ruleSection, VariableSet variables)
    {
        var responseSection = ruleSection.GetSingleChildSection(Constants.SectionName.Response);

        var responseWriter = new ResponseWriter(
            GetHttpCode(responseSection), 
            GetBodyWriter(responseSection, variables), 
            GetHeadersWriter(responseSection, variables),
            GetDelay(responseSection));

        return responseWriter;
    }

    private static TimeSpan? GetDelay(FileSection responseSection)
    {
        var block = responseSection.GetBlock(Constants.BlockName.Response.Delay);

        if (block == null)
            return null;

        var delayStr = block.GetSingleLine();
        var delay = PrettyTimespanParser.Parse(delayStr);

        return delay;
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

    private BodyWriter? GetBodyWriter(FileSection responseSection, VariableSet variables)
    {
        BodyWriter? bodyWriter = null;
        var bodyBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Body);

        if (bodyBlock != null)
        {
            var parts = _valuePartsCreator.Create(bodyBlock.GetStringValue(), variables);
            bodyWriter = new BodyWriter(new ValuePartSet(parts));
        }

        return bodyWriter;
    }

    private HeadersWriter? GetHeadersWriter(FileSection responseSection, VariableSet variables)
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

                var parts = _valuePartsCreator.Create(headerPattern, variables);

                headers.Add(headerName, new ValuePartSet(parts));
            }

            headersWriter = new HeadersWriter(headers);
        }

        return headersWriter;
    }

    private static int ParseHttpCode(string str)
    {
        if (!int.TryParse(str, out int httpCode))
            throw new Exception($"Invalid http code: '{str}'");
        return httpCode;
    }
}
