using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating;

internal interface IGeneratingPrettyFunction
{
    object? Generate(RequestData request);
}

