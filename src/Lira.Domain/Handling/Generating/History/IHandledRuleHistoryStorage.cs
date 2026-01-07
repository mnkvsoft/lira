namespace Lira.Domain.Handling.Generating.History;

public interface IHandledRuleHistoryStorage
{
    IEnumerable<RuleHistoryItem> GetHistory(RuleName ruleName);
}

public record RuleHistoryItem(DateTime HandleTime, RequestData Request, Response Result);