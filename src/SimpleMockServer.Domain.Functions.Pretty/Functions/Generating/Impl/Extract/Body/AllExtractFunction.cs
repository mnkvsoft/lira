using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body.Extensions;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body;

public class AllExtractFunction : IBodyExtractFunction, IGeneratingPrettyFunction
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
