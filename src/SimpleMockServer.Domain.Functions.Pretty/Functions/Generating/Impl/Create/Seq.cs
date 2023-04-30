using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Create;

internal class Seq : IGeneratingPrettyFunction
{
    private static long _counter;

    public static string Name => "seq";

    public object? Generate(RequestData request)
    {
        return Interlocked.Increment(ref _counter);
    }
}
