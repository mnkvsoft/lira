namespace Lira.Domain.Extensions;

public static class RequestDataExtensions
{
    public static string ReadBody(this RequestData request)
    {
        request.Body.Position = 0;
        var stream = new StreamReader(request.Body);
        var body = stream.ReadToEnd();

        return body;
    }
}
