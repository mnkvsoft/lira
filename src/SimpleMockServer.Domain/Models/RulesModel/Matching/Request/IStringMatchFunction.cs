namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Request;

public interface IStringMatchFunction
{
    bool IsMatch(string? value);
}
