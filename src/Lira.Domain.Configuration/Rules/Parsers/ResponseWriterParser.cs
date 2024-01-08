using Lira.Common.PrettyParsers;
using Lira.Domain.Generating.Writers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules.Parsers;

class ResponseStrategyParser
{
    private readonly GeneratingHttpDataParser _httpDataParser;

    public ResponseStrategyParser(GeneratingHttpDataParser httpDataParser)
    {
        _httpDataParser = httpDataParser;
    }

    public async Task<ResponseStrategy> Parse(FileSection ruleSection, ParsingContext parsingContext)
    {
        var responseSection = ruleSection.GetSingleChildSectionOrNull(Constants.SectionName.Response);

        if (responseSection == null)
        {
            return new ResponseStrategy.Normal(
                Delay: null,
                Code: 200,
                BodyGenerator: null,
                HeadersGenerator: null);
        }

        var delay = GetDelay(responseSection);
        if (responseSection.ExistBlock(Constants.BlockName.Response.Abort))
        {
            var blocks = responseSection.GetBlocks(
                Constants.BlockName.Response.Body,
                Constants.BlockName.Response.Code,
                Constants.BlockName.Response.Headers);

            if (blocks.Count > 1)
            {
                throw new Exception($"if block '{Constants.BlockName.Response.Abort}' is defined, " +
                                    $"then there should not be blocks: " +
                                    $"{Constants.BlockName.Response.Body}, " +
                                    $"{Constants.BlockName.Response.Code}, " +
                                    $"{Constants.BlockName.Response.Headers}, " +
                                    $"but there are: {string.Join(", ", blocks.Select(b => b.Name))}");
            }

            return new ResponseStrategy.Abort(delay);
        }

        return new ResponseStrategy.Normal(
            delay,
            GetHttpCode(responseSection),
            await GetBodyGenerator(responseSection, parsingContext),
            await GetHeadersGenerator(responseSection, parsingContext));
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
        if (responseSection.LinesWithoutBlock.Count > 0)
            return ParseHttpCode(responseSection.GetSingleLine());

        var codeBlock = responseSection.GetBlock(Constants.BlockName.Response.Code);

        if (codeBlock == null)
            return 200;

        return ParseHttpCode(codeBlock.GetSingleLine());
    }

    private async Task<BodyGenerator?> GetBodyGenerator(FileSection responseSection, ParsingContext parsingContext)
    {
        BodyGenerator? bodyWriter = null;
        var bodyBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Body);

        if (bodyBlock != null)
        {
            var parts = await _httpDataParser.ParseBody(bodyBlock, parsingContext);
            bodyWriter = new BodyGenerator(parts.WrapToTextParts());
        }

        return bodyWriter;
    }

    private async Task<HeadersGenerator?> GetHeadersGenerator(FileSection responseSection, ParsingContext parsingContext)
    {
        var headersBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Headers);

        if (headersBlock != null)
            return new HeadersGenerator(await _httpDataParser.ParseHeaders(headersBlock, parsingContext));

        return null;
    }

    private static int ParseHttpCode(string? str)
    {
        if (!int.TryParse(str, out var httpCode))
            throw new Exception($"Invalid http code: '{str}'");
        return httpCode;
    }
}