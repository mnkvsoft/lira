using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl;

internal class Guid : IGeneratingPrettyFunction
{
    public static string Name => "guid";

    public object? Generate(HttpRequest request)
    {
        return System.Guid.NewGuid();
    }
}
