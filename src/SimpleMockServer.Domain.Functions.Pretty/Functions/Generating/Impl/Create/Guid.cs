using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Create;

internal class Guid : IGeneratingPrettyFunction
{
    public static string Name => "guid";

    public object? Generate(RequestData request)
    {
        return System.Guid.NewGuid();
    }
}
