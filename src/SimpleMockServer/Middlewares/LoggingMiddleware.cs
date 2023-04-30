using System.Text;

namespace SimpleMockServer.Middlewares;

public class LoggingMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<LoggingMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        //First, get the incoming request
        var request = await FormatRequest(context.Request);
        _logger.LogInformation(request);

        //Copy a pointer to the original response body stream
        var originalBodyStream = context.Response.Body;

        //Create a new memory stream...
        using (var responseBody = new MemoryStream())
        {
            //...and use that for the temporary response body
            context.Response.Body = responseBody;

            //Continue down the Middleware pipeline, eventually returning to this class
            await _next(context);

            //Format the response from the server
            var response = await FormatResponse(context.Response);
            _logger.LogInformation(response);

            //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        //This line allows us to set the reader for the request back at the beginning of its stream.
        request.EnableBuffering();

        //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];

        //...Then we copy the entire request stream into the new buffer.
        await request.Body.ReadAsync(buffer, 0, buffer.Length);

        //We convert the byte[] into a string using UTF8 encoding...
        var bodyAsText = Encoding.UTF8.GetString(buffer);

        //..and finally, assign the read body back to the request body, which is allowed because of EnableBuffering()
        request.Body.Position = 0;

        var sb = new StringBuilder();

        sb.AppendLine("Request:");
        sb.AppendLine($"{request.Method} {request.Path}");

        if (request.QueryString.HasValue)
        {
            sb.AppendLine($"?{request.QueryString}");
        }

        if (request.Headers.Any())
        {
            sb.AppendLine();
            foreach (var header in request.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        if (bodyAsText.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine(bodyAsText);
        }

        return sb.ToString();
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
        //We need to read the response stream from the beginning...
        response.Body.Seek(0, SeekOrigin.Begin);

        //...and copy it into a string
        string text = await new StreamReader(response.Body).ReadToEndAsync();

        //We need to reset the reader for the response so that the client can read it.
        response.Body.Seek(0, SeekOrigin.Begin);

        //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)

        var sb = new StringBuilder();
        sb.AppendLine("Response:");
        sb.AppendLine(((int)response.StatusCode).ToString());

        if (response.Headers.Any())
        {
            sb.AppendLine();
            foreach (var header in response.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        if (text.Length > 0)
        {
            sb.AppendLine(text);
        }

        return sb.ToString();
    }
}
