using SimpleMockServer.Domain.Extensions;
using SimpleMockServer.Domain.Matching.Request.Matchers;

namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Generating.Impl.Extract.Body;

public class AllExtractFunction : IBodyExtractFunction, IObjectTextPart
{
    public static string Name => "req.body.all";

    public string? Extract(string? value)
    {
        return value;
    }

    public object Get(RequestData request)
    {
        return request.ReadBody();
    }
}
