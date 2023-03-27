using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.ConfigurationProviding.Rules;

public record RuleWithExtInfo(Rule Rule, string FileName)
{
    public override string ToString()
    {
        return $"{Rule.Name} (file: {FileName})";
    }
}