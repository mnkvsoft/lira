using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl;

internal class Seq : IGeneratingPrettyFunction
{
    private static long _counter;

    public static string Name => "seq";

    public object? Generate(HttpRequest request)
    {
        return Interlocked.Increment(ref _counter); 
    }
}
