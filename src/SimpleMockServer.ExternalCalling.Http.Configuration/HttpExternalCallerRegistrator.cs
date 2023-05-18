using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain;
using SimpleMockServer.Domain.Configuration;
using SimpleMockServer.Domain.Configuration.Rules;
using SimpleMockServer.Domain.Configuration.Rules.Parsers;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Transform.Format;
using SimpleMockServer.ExternalCalling.Http.Caller;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ExternalCalling.Http.Configuration;

class BlockName
{
    public const string Headers = "headers";
    public const string Body = "body";
}

public class HttpExternalCallerRegistrator : IExternalCallerRegistrator
{
    public string Name => "http";
    public GeneratingHttpDataParser _generatingHttpDataParser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITextPartsParser _partsParser;


    public HttpExternalCallerRegistrator(
        GeneratingHttpDataParser generatingHttpDataParser,
        IHttpClientFactory httpClientFactory,
        ITextPartsParser partsParser)
    {
        _generatingHttpDataParser = generatingHttpDataParser;
        _httpClientFactory = httpClientFactory;
        _partsParser = partsParser;
    }

    public async Task<IExternalCaller> Create(FileSection section, IParsingContext parsingContext)
    {
        var methodAndUrl = section.GetSingleLine();
        var (methodStr, urlStr) = methodAndUrl.SplitToTwoPartsRequired(" ");

        var method = methodStr.ToHttpMethod();
        var urlParts = await _partsParser.Parse(urlStr, parsingContext);

        var bodyRawText = section.GetStringValueFromBlockOrEmpty(BlockName.Body);
        var bodyParts = await _partsParser.Parse(bodyRawText, parsingContext);

        var headerBlock = section.GetBlockOrNull(BlockName.Headers);
        var headers = headerBlock == null ? null : await _generatingHttpDataParser.ParseHeaders(headerBlock, parsingContext);

        var caller = new HttpExternalCaller(
            _httpClientFactory, 
            method, 
            ObjectTextPartsExtensions.WrapToTextParts(urlParts), 
            ObjectTextPartsExtensions.WrapToTextParts(bodyParts), 
            headers);
        
        return caller;
    }

    public IReadOnlyCollection<string> GetSectionKnowsBlocks() => BlockNameHelper.GetBlockNames<BlockName>();
}
