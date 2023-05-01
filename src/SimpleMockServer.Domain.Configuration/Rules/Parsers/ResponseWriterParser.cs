using SimpleMockServer.Domain.Generating.Writers;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers;

class ResponseWriterParser
{
    private readonly GeneratingHttpDataParser _httpDataParser;

    public ResponseWriterParser(GeneratingHttpDataParser httpDataParser)
    {
        _httpDataParser = httpDataParser;
    }

    public Delayed<ResponseWriter> Parse(FileSection ruleSection, VariableSet variables)
    {
        var responseSection = ruleSection.GetSingleChildSection(Constants.SectionName.Response);

        var responseWriter = new Delayed<ResponseWriter>(
            new ResponseWriter(
                GetHttpCode(responseSection),
                GetBodyWriter(responseSection, variables),
                GetHeadersWriter(responseSection, variables)),
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
            var textGenerator = _httpDataParser.ParseBody(bodyBlock, variables);
            bodyWriter = new BodyWriter(textGenerator);
        }

        return bodyWriter;
    }

    private HeadersWriter? GetHeadersWriter(FileSection responseSection, VariableSet variables)
    {
        var headersBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Headers);

        if (headersBlock != null)
            return new HeadersWriter(_httpDataParser.ParseHeaders(headersBlock, variables));

        return null;
    }

    private static int ParseHttpCode(string str)
    {
        if (!int.TryParse(str, out var httpCode))
            throw new Exception($"Invalid http code: '{str}'");
        return httpCode;
    }
}
