using Lira.Common.Extensions;
using Lira.Domain;
using Lira.Domain.Configuration;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling;
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
    private readonly HeadersParser _headersParser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITextPartsParser _partsParser;


    public HttpSystemActionRegistrator(
        HeadersParser headersParser,
        IHttpClientFactory httpClientFactory,
        ITextPartsParser partsParser)
    {
        _headersParser = headersParser;
        _httpClientFactory = httpClientFactory;
        _partsParser = partsParser;
    }

    public async Task<IAction> Create(FileSection section, IParsingContext parsingContext)
    {
        var methodAndUrl = section.GetSingleLine();
        var (methodStr, urlStr) = methodAndUrl.SplitToTwoPartsRequired(" ");

        var method = methodStr.ToHttpMethod();
        var urlParts = await _partsParser.Parse(urlStr, parsingContext);

        var bodyBlock = section.GetBlockOrNull(BlockName.Body);
        var bodyParts = bodyBlock == null ? null : await _partsParser.Parse(bodyBlock.GetLinesAsString(), parsingContext);

        var headerBlock = section.GetBlockOrNull(BlockName.Headers);
        var headers = headerBlock == null ? null : await _headersParser.ParseHeaders(headerBlock, parsingContext);

        var caller = new HttpAction(
            _httpClientFactory,
            method,
            urlParts.WrapToTextParts(),
            bodyParts?.WrapToTextParts(),
            headers);

        return caller;
    }
}
