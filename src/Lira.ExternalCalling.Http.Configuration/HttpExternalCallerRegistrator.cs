using Lira.Domain;
using Lira.Common.Extensions;
using Lira.Domain.Actions;
using Lira.Domain.Configuration;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.ExternalCalling.Http.Caller;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.ExternalCalling.Http.Configuration;

class BlockName
{
    public const string Headers = "headers";
    public const string Body = "body";
}

public class HttpSystemActionRegistrator : ISystemActionRegistrator
{
    public string Name => "call.http";
    public GeneratingHttpDataParser _generatingHttpDataParser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITextPartsParser _partsParser;


    public HttpSystemActionRegistrator(
        GeneratingHttpDataParser generatingHttpDataParser,
        IHttpClientFactory httpClientFactory,
        ITextPartsParser partsParser)
    {
        _generatingHttpDataParser = generatingHttpDataParser;
        _httpClientFactory = httpClientFactory;
        _partsParser = partsParser;
    }

    public async Task<IAction> Create(FileSection section, IParsingContext parsingContext)
    {
        var methodAndUrl = section.GetSingleLine();
        var (methodStr, urlStr) = methodAndUrl.SplitToTwoPartsRequired(" ");

        var method = methodStr.ToHttpMethod();
        var urlParts = await _partsParser.Parse(urlStr, parsingContext);

        var bodyRawText = section.GetStringValueFromBlockOrEmpty(BlockName.Body);
        var bodyParts = await _partsParser.Parse(bodyRawText, parsingContext);

        var headerBlock = section.GetBlockOrNull(BlockName.Headers);
        var headers = headerBlock == null ? null : await _generatingHttpDataParser.ParseHeaders(headerBlock, parsingContext);

        if(headers?.FirstOrDefault(x => x.Name == "Content-Type") == null)
            throw new Exception("Header Content-Type is required");
        
        var caller = new HttpAction(
            _httpClientFactory, 
            method, 
            urlParts.WrapToTextParts(), 
            bodyParts.WrapToTextParts(), 
            headers);
        
        return caller;
    }
}
