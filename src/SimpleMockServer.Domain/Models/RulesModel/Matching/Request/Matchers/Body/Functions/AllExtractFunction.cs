namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body.Functions;

public class AllExtractFunction : IExtractFunction
{
    public string? Extract(string? value)
    {
        return value;
    }
}