using System.Text;
using Microsoft.Extensions.Logging;

namespace Lira.ExternalCalling.Http.Caller;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Request:");
        sb.AppendLine($"{request.Method} {request.RequestUri}");
        sb.AppendLine();

        foreach (var header in request.Headers)
        {
            sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        if (request.Content != null)
        {
            sb.AppendLine();
            sb.AppendLine(await request.Content.ReadAsStringAsync(cancellationToken));
        }

        _logger.LogInformation(sb.ToString());

        var response = await base.SendAsync(request, cancellationToken);

        sb.Clear();
        sb.AppendLine("Response:");
        sb.AppendLine(((int)response.StatusCode).ToString());

        var headers = response.GetAllHeaders();
        if (headers.Any())
        {
            sb.AppendLine();
            foreach (var header in headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        sb.AppendLine();
        var value = await response.Content.ReadAsStringAsync(cancellationToken);
        sb.AppendLine(value);

        _logger.LogInformation(sb.ToString());

        return response;
    }
}