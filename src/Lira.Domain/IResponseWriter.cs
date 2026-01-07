using System.Text;

namespace Lira.Domain;

public interface IResponseWriter
{
    void WriteCode(int code);
    void WriteHeader(Header header);
    Task WriteBody(string part, Encoding encoding);

    void Abort();
}

internal static class ResponseWriterExtensions
{
    public static Task Write404(this IResponseWriter responseWriter, string message)
    {
        responseWriter.WriteCode(404);
        return responseWriter.WriteBody(message);
    }

    private static Task WriteBody(this IResponseWriter responseWriter, string part)
        => responseWriter.WriteBody(part, Encoding.UTF8);
}