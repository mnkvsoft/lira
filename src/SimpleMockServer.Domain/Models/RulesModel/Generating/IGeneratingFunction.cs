using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Generating;

public interface IGeneratingFunction 
{
    string? Generate(HttpRequest request);
}
