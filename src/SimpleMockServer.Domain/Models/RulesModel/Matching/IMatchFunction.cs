namespace SimpleMockServer.Domain.Models.RulesModel.Matching;

public interface IMatchFunction
{
    bool IsMatch(string? value);
}
