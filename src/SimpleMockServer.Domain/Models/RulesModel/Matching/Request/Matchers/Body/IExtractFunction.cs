namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

public interface IBodyExtractFunction
{
    string? Extract(string? body);
}
