using Lira.Common.Exceptions;
using Lira.Contracts;
using Lira.Domain;
using Lira.Domain.Handling.Generating.History;
using Microsoft.AspNetCore.Mvc;
using Response = Lira.Domain.Response;

namespace Lira.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    [HttpGet("history/{ruleName}")]
    public GetHistoryResponse GetHistory([FromServices] IHandledRuleHistoryStorage historyStorage, string ruleName)
    {
        var name = new RuleName(ruleName);

        var items = historyStorage.GetHistory(name);
        return new GetHistoryResponse(items.Select(ToInvoke).ToArray());
    }

    private static readonly IReadOnlyDictionary<string, string?> EmptyDictionary = new Dictionary<string, string?>();

    private RuleInvoke ToInvoke(RuleHistoryItem item)
    {
        Result result = item.Result switch
        {
            Response.Normal res => new Result(
                Response: new Contracts.Response(
                    res.Code,
                    res.Headers ?? EmptyDictionary,
                    res.Body),
                Fault: false),
            Domain.Response.Fault => new Result(
                Response: null,
                Fault: true),
            _ => throw new UnsupportedInstanceType(item.Result)
        };

        var req = item.Request;
        return new RuleInvoke(
            item.HandleTime,
            new Request(
                req.Path.ToString(),
                req.QueryString.ToString(),
                req.Headers.ToDictionary(x => x.Key, x => x.Value.ToString())),
            result);
    }
}