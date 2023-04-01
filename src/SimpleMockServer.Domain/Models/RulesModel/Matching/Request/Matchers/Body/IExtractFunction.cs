namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

public interface IExtractFunction
{
    string? Extract(string? value);
}
