﻿using Lira.Domain.Generating.Writers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Generating;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class ResponseStrategyParser
{
    private readonly HeadersParser _headersParser;
    private readonly ITextPartsParser _partsParser;
    private readonly DelayGeneratorParser _delayGeneratorParser;

    public ResponseStrategyParser(HeadersParser headersParser, ITextPartsParser partsParser, DelayGeneratorParser delayGeneratorParser)
    {
        _headersParser = headersParser;
        _partsParser = partsParser;
        _delayGeneratorParser = delayGeneratorParser;
    }

    public async Task<ResponseStrategy> Parse(FileSection ruleSection, ParsingContext parsingContext)
    {
        var responseSection = ruleSection.GetSingleChildSectionOrNull(Constants.SectionName.Response);

        if (responseSection == null)
        {
            return new ResponseStrategy.Normal(
                GetDelay: null,
                StaticHttCodeGenerator.Code200,
                BodyGenerator: null,
                HeadersGenerator: null);
        }

        if (responseSection.Blocks.Count == 0)
        {
            if (responseSection.LinesWithoutBlock.Count == 0)
            {
                var strCode = responseSection.Key;
                if(string.IsNullOrEmpty(strCode))
                    throw new Exception("No response section found");

                return new ResponseStrategy.Normal(
                    GetDelay: null,
                    new StaticHttCodeGenerator(strCode.ToHttpCode()),
                    BodyGenerator: null,
                    HeadersGenerator: null);
            }

            if(responseSection.LinesWithoutBlock.Count == 1 && int.TryParse(responseSection.GetSingleLine(), out var code))
            {
                return new ResponseStrategy.Normal(
                    GetDelay: null,
                    new StaticHttCodeGenerator(code),
                    BodyGenerator: null,
                    HeadersGenerator: null);
            }

            string text = responseSection.GetLinesWithoutBlockAsString();
            var parts = await _partsParser.Parse(text, parsingContext);

            return new ResponseStrategy.Normal(
                GetDelay: null,
                StaticHttCodeGenerator.Code200,
                new BodyGenerator(parts.WrapToTextParts()),
                HeadersGenerator: null);
        }

        var delayGenerator = await _delayGeneratorParser.Parse(responseSection, parsingContext);
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

            return new ResponseStrategy.Abort(delayGenerator);
        }

        return new ResponseStrategy.Normal(
            delayGenerator,
            await GetHttpCodeGenerator(responseSection, parsingContext),
            await GetBodyGenerator(responseSection, parsingContext),
            await GetHeadersGenerator(responseSection, parsingContext));
    }

    private async Task<IHttCodeGenerator> GetHttpCodeGenerator(FileSection responseSection, ParsingContext parsingContext)
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

    private async Task<HeadersGenerator?> GetHeadersGenerator(FileSection responseSection, ParsingContext parsingContext)
    {
        var headersBlock = responseSection.GetBlockOrNull(Constants.BlockName.Response.Headers);

        if (headersBlock != null)
            return new HeadersGenerator(await _headersParser.ParseHeaders(headersBlock, parsingContext));

        return null;
    }
}