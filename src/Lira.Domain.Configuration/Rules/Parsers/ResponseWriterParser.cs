using System.Text;
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
    private readonly IResponseGenerationHandlerFactory _responseGenerationHandlerFactory;

    public ResponseGenerationHandlerParser(
        HeadersParser headersParser,
        ITextPartsParser partsParser,
        IResponseGenerationHandlerFactory responseGenerationHandlerFactory)
    {
        _headersParser = headersParser;
        _partsParser = partsParser;
        _responseGenerationHandlerFactory = responseGenerationHandlerFactory;
    }

    public async Task<IHandler> Parse(WriteHistoryMode writeHistoryMode, FileSection responseSection, ParsingContext parsingContext)
    {
        if (responseSection.Blocks.Count == 0)
        {
            if (responseSection.LinesWithoutBlock.Count == 0)
            {
                var strCode = responseSection.Key;
                if (string.IsNullOrEmpty(strCode))
                    throw new Exception("No response section found");

                return _responseGenerationHandlerFactory.CreateNormal(
                    writeHistoryMode,
                    new StaticHttCodeGenerator(strCode.ToHttpCode()),
                    bodyGenerator: null,
                    headersGenerator: null);
            }

            if (responseSection.LinesWithoutBlock.Count == 1 &&
                int.TryParse(responseSection.GetSingleLine(), out var code))
            {
                return _responseGenerationHandlerFactory.CreateNormal(
                    writeHistoryMode,
                    new StaticHttCodeGenerator(code),
                    bodyGenerator: null,
                    headersGenerator: null);
            }

            string text = responseSection.GetLinesWithoutBlockAsString();
            var parts = await _partsParser.Parse(text, parsingContext);

            return _responseGenerationHandlerFactory.CreateNormal(
                writeHistoryMode,
                StaticHttCodeGenerator.Code200,
                new BodyGenerator(parts.WrapToTextParts()),
                headersGenerator: null);
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

            return _responseGenerationHandlerFactory.CreateAbort(writeHistoryMode);
        }

        return _responseGenerationHandlerFactory.CreateNormal(
            writeHistoryMode,
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

        var key = codeBlock.Key;
        var content = codeBlock.GetLinesAsString();

        if(key != null && !string.IsNullOrEmpty(content))
            throw new Exception($"Duplicate code: {key}, {content}");

        var code = key ?? content;

        if (string.IsNullOrWhiteSpace(code))
            throw new Exception($"Empty http code: '{code}'");

        var textParts = await _partsParser.Parse(code, parsingContext);
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