using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling.Generating;
using Lira.Domain.Handling.Generating.ResponseStrategies;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Fault;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal.Generators;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class ResponseStrategyParser
{
    private readonly HeadersParser _headersParser;
    private readonly ITextPartsParser _partsParser;

    public ResponseStrategyParser(
        HeadersParser headersParser,
        ITextPartsParser partsParser)
    {
        _headersParser = headersParser;
        _partsParser = partsParser;
    }

    public async Task<IResponseStrategy> Parse(FileSection responseSection, ParsingContext parsingContext)
    {
        if (responseSection.Blocks.Count == 0)
        {
            if (responseSection.LinesWithoutBlock.Count == 0)
            {
                var strCode = responseSection.Key;
                if (string.IsNullOrEmpty(strCode))
                    throw new Exception("No response section found");

                return new NormalResponseStrategy(new StaticHttCodeGenerator(strCode.ToHttpCode()));
            }

            if (responseSection.LinesWithoutBlock.Count == 1 &&
                int.TryParse(responseSection.GetSingleLine(), out var code))
            {
                return new NormalResponseStrategy(new StaticHttCodeGenerator(code));
            }

            string text = responseSection.GetLinesWithoutBlockAsString();
            var parts = await _partsParser.Parse(text, parsingContext);

            return new NormalResponseStrategy(
                StaticHttCodeGenerator.Code200,
                new BodyGenerator(parts.WrapToTextParts()));
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

            return FaultResponseStrategy.Instance;
        }

        var codeGenerator = await GetHttpCodeGenerator(responseSection, parsingContext);
        var bodyGenerator = await GetBodyGenerator(responseSection, parsingContext);
        var headersGenerator = await GetHeadersGenerator(responseSection, parsingContext);

        return new NormalResponseStrategy(
            codeGenerator,
            bodyGenerator,
            headersGenerator);
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

    private async Task<IBodyGenerator?> GetBodyGenerator(FileSection responseSection, ParsingContext parsingContext)
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

    private async Task<IHeadersGenerator?> GetHeadersGenerator(FileSection responseSection,
        ParsingContext parsingContext)
    {
        var headersBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Headers);

        if (headersBlock != null)
            return new HeadersGenerator(await _headersParser.ParseHeaders(headersBlock, parsingContext));

        return null;
    }
}