namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;

public interface IExtractFunction
{
    string? Extract(string? value);
}
