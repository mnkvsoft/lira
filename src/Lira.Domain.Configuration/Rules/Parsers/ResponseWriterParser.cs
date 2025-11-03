using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling.Generating;
using Lira.Domain.Handling.Generating.Writers;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class ResponseGenerationHandlerParser
{
    private readonly HeadersParser _headersParser;
    private readonly ITextPartsParser _partsParser;

    public ResponseGenerationHandlerParser(HeadersParser headersParser, ITextPartsParser partsParser)
    {
        _headersParser = headersParser;
        _partsParser = partsParser;
    }

    public async Task<ResponseGenerationHandler> Parse(FileSection responseSection, ParsingContext parsingContext)
    {
        if (responseSection.Blocks.Count == 0)
        {
            if (responseSection.LinesWithoutBlock.Count == 0)
            {
                var strCode = responseSection.Key;
                if (string.IsNullOrEmpty(strCode))
                    throw new Exception("No response section found");

                return new ResponseGenerationHandler.Normal(
                    new StaticHttCodeGenerator(strCode.ToHttpCode()),
                    BodyGenerator: null,
                    HeadersGenerator: null);
            }

            if (responseSection.LinesWithoutBlock.Count == 1 &&
                int.TryParse(responseSection.GetSingleLine(), out var code))
            {
                return new ResponseGenerationHandler.Normal(
                    new StaticHttCodeGenerator(code),
                    BodyGenerator: null,
                    HeadersGenerator: null);
            }

            string text = responseSection.GetLinesWithoutBlockAsString();
            var part = await _partsParser.Parse(text, parsingContext);

            return new ResponseGenerationHandler.Normal(
                StaticHttCodeGenerator.Code200,
                new BodyGenerator(part.WrapToTextParts()),
                HeadersGenerator: null);
        }

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

            return new ResponseGenerationHandler.Abort();
        }

        return new ResponseGenerationHandler.Normal(
            await GetHttpCodeGenerator(responseSection, parsingContext),
            await GetBodyGenerator(responseSection, parsingContext),
            await GetHeadersGenerator(responseSection, parsingContext));
    }

    private async Task<IHttCodeGenerator> GetHttpCodeGenerator(FileSection responseSection,
        ParsingContext parsingContext)
    {
        var codeBlock = responseSection.GetBlock(Constants.BlockName.Response.Code);

        if (codeBlock == null)
            return StaticHttCodeGenerator.Code200;

        var str = codeBlock.GetLinesAsString();

        if (string.IsNullOrWhiteSpace(str))
            throw new Exception($"Empty http code: '{str}'");

        var textParts = await _partsParser.Parse(str, parsingContext);
        return new DynamicHttCodeGenerator(textParts.WrapToTextParts());
    }

    private async Task<BodyGenerator?> GetBodyGenerator(FileSection responseSection, ParsingContext parsingContext)
    {
        BodyGenerator? bodyWriter = null;
        var bodyBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Body);

        if (bodyBlock != null)
        {
            var parts = await _partsParser.Parse(bodyBlock.GetLinesAsString(), parsingContext);
            bodyWriter = new BodyGenerator(parts.WrapToTextParts());
        }

        return bodyWriter;
    }

    private async Task<HeadersGenerator?> GetHeadersGenerator(FileSection responseSection,
        ParsingContext parsingContext)
    {
        var headersBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Headers);

        if (headersBlock != null)
            return new HeadersGenerator(await _headersParser.ParseHeaders(headersBlock, parsingContext));

        return null;
    }
}