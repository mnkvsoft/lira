using Lira.Domain.Generating.Writers;
using Lira.Domain.Configuration.PrettyParsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules.Parsers;

class ResponseWriterParser
{
    private readonly GeneratingHttpDataParser _httpDataParser;

    public ResponseWriterParser(GeneratingHttpDataParser httpDataParser)
    {
        _httpDataParser = httpDataParser;
    }

    public async Task<Delayed<ResponseWriter>> Parse(FileSection ruleSection, ParsingContext parsingContext)
    {
        var responseSection = ruleSection.GetSingleChildSection(Constants.SectionName.Response);

        var responseWriter = new Delayed<ResponseWriter>(
            new ResponseWriter(
                GetHttpCode(responseSection),
                await GetBodyWriter(responseSection, parsingContext),
                await GetHeadersWriter(responseSection, parsingContext)),
            GetDelay(responseSection));

        return responseWriter;
    }

    private static TimeSpan? GetDelay(FileSection responseSection)
    {
        responseSection.AssertContainsOnlyKnownBlocks(BlockNameHelper.GetBlockNames<Constants.BlockName.Response>());
        
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

    private async Task<BodyWriter?> GetBodyWriter(FileSection responseSection, ParsingContext parsingContext)
    {
        BodyWriter? bodyWriter = null;
        var bodyBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Body);

        if (bodyBlock != null)
        {
            var parts = await _httpDataParser.ParseBody(bodyBlock, parsingContext);
            bodyWriter = new BodyWriter(parts.WrapToTextParts());
        }

        return bodyWriter;
    }

    private async Task<HeadersWriter?> GetHeadersWriter(FileSection responseSection, ParsingContext parsingContext)
    {
        var headersBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Headers);

        if (headersBlock != null)
            return new HeadersWriter(await _httpDataParser.ParseHeaders(headersBlock, parsingContext));

        return null;
    }

    private static int ParseHttpCode(string str)
    {
        if (!int.TryParse(str, out var httpCode))
            throw new Exception($"Invalid http code: '{str}'");
        return httpCode;
    }
}
