using System.Text;
using Lira.Domain.Handling.Generating.History;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal.Generators;
using Lira.Domain.Utils;
using Microsoft.Net.Http.Headers;

namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal;

record NormalResponseGenerationHandler(
    WriteStatDependencies HistoryDependencies,
    IHttCodeGenerator CodeGenerator,
    BodyGenerator? BodyGenerator,
    HeadersGenerator? HeadersGenerator) : IHandler
{
    public async Task Handle(HttpContextData httpContextData)
    {
        var context = httpContextData.RuleExecutingContext;
        var response = httpContextData.Response;

        response.NeedSaveData = HistoryDependencies.WriteHistoryMode is WriteHistoryMode.Write;

        var statusCode = CodeGenerator.Generate(httpContextData.RuleExecutingContext);
        response.StatusCode = statusCode;

        Header? contentTypeHeader = null;
        if (HeadersGenerator != null)
        {
            foreach (var header in HeadersGenerator.Create(context))
            {
                if (header.Name == HeaderNames.ContentType)
                    contentTypeHeader = header;
                else
                    response.AddHeader(header);
            }
        }

        Encoding? encoding = null;

        if (contentTypeHeader != null)
        {
            var charset = contentTypeHeader.Value.ExtractCharset();

            if (charset != null)
            {
                if (!ContentTypeHeaderUtils.TryGetEncoding(charset, out encoding))
                {
                    throw new Exception(
                        $"The charset '{charset}' is not supported. " +
                        $"Supported encoding types are: {string.Join(", ", ContentTypeHeaderUtils.AvailableEncodings.Select(x => x.WebName))}");
                }
            }

            response.AddHeader(contentTypeHeader.Value);
        }

        if (BodyGenerator != null)
        {
            foreach (string bodyPart in BodyGenerator.Create(context))
            {
                await response.WriteBody(bodyPart, encoding ?? Encoding.UTF8);
            }
        }

        if (HistoryDependencies.WriteHistoryMode is WriteHistoryMode.Write writeHistoryMode)
        {
            HistoryDependencies.Storage.Add(
                writeHistoryMode.RuleName,
                DateTime.UtcNow,
                httpContextData.RuleExecutingContext.RequestData,
                new RequestHandleResult.Response(response.StatusCode, response.Headers, response.Body));
        }
    }
}