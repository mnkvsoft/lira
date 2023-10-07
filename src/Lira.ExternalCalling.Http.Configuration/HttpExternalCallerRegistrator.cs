using Lira.Domain;
using Lira.Common.Extensions;
using Lira.Domain.Configuration;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.ExternalCalling.Http.Caller;
using Lira.FileSectionFormat;

namespace Lira.ExternalCalling.Http.Configuration;

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

        if(headers?.FirstOrDefault(x => x.Name == Header.ContentType) == null)
            throw new Exception("Header Content-Type is required");
        
        var caller = new HttpExternalCaller(
            _httpClientFactory, 
            method, 
            urlParts.WrapToTextParts(), 
            bodyParts.WrapToTextParts(), 
            headers);
        
        return caller;
    }
}
