namespace Lira.Domain.Handling.Generating.History;

internal interface IHandledRuleHistoryStorage
{
    IEnumerable<RuleHistoryItem> GetHistory(RuleName ruleName);
}

record RuleHistoryItem(DateTime HandleTime, RequestData Request, RequestHandleResult Result);