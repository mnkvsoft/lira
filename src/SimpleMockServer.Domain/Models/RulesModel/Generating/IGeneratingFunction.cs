namespace SimpleMockServer.Domain.Models.RulesModel.Generating;

public interface IGeneratingFunction 
{
    string? Generate(RequestData request);
}
