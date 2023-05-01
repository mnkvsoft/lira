using SimpleMockServer.Domain.Matching.Request.Matchers.Body;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body.Extensions;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body;

public class AllExtractFunction : IBodyExtractFunction, IGeneratingFunction
{
    public static string Name => "extract.body.all";

    public string? Extract(string? value)
    {
        return value;
    }

    public object? Generate(RequestData request)
    {
        return request.ReadBody();
    }
}
