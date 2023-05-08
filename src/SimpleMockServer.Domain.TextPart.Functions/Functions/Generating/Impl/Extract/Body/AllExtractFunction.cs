using SimpleMockServer.Domain.Matching.Request.Matchers.Body;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body.Extensions;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body;

public class AllExtractFunction : IBodyExtractFunction, IObjectTextPart
{
    public static string Name => "read.req.body.all";

    public string? Extract(string? value)
    {
        return value;
    }

    public object Get(RequestData request)
    {
        return request.ReadBody();
    }
}
