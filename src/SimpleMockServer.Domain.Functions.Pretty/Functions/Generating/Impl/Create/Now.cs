using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Create;

internal class Now : IGeneratingPrettyFunction
{
    public static string Name => "now";

    public object? Generate(RequestData request)
    {
        return DateTime.Now;
    }
}
