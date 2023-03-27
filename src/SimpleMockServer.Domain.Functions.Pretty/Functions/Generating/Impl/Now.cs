using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl;

internal class Now : IGeneratingPrettyFunction
{
    public static string Name => "now";

    public object? Generate(HttpRequest request)
    {
        return DateTime.Now;
    }
}
