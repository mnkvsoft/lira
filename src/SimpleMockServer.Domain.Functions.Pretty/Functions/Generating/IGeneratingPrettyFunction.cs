using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating;

internal interface IGeneratingPrettyFunction
{
    object? Generate(HttpRequest request);
}

