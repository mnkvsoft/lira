using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body.Functions;

public class AllExtractFunction : IExtractFunction
{
    public string? Extract(string? value)
    {
        return value;
    }
}