using System.Text;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal.Generators;
using Lira.Domain.Utils;
using Microsoft.Net.Http.Headers;

namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal;

public record NormalResponseStrategy(
    IHttCodeGenerator CodeGenerator,
    IBodyGenerator? BodyGenerator = null,
    IHeadersGenerator? HeadersGenerator = null) : IResponseStrategy
{
    async Task IResponseStrategy.Handle(RuleExecutingContext ruleExecutingContext, IResponseWriter responseWriter)
    {
        var statusCode = CodeGenerator.Generate(ruleExecutingContext);
        responseWriter.WriteCode(statusCode);

        Header? contentTypeHeader = null;
        if (HeadersGenerator != null)
        {
            foreach (var header in HeadersGenerator.Create(ruleExecutingContext))
            {
                if (header.Name == HeaderNames.ContentType)
                    contentTypeHeader = header;
                else
                    responseWriter.WriteHeader(header);
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

            responseWriter.WriteHeader(contentTypeHeader.Value);
        }

        if (BodyGenerator != null)
        {
            foreach (string bodyPart in BodyGenerator.Create(ruleExecutingContext))
            {
                await responseWriter.WriteBody(bodyPart, encoding ?? Encoding.UTF8);
            }
        }
    }
}