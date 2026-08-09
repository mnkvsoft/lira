using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lira.Domain;

public interface IRequestHandlerBuilder
{
    int Count { get; }
    IRequestHandler Build();

    public void AddRule(
        string ruleInfo,
        IReadOnlyCollection<IRequestMatcher> requestMatchers,
        RuleMiddlewares middlewares);
}

public class RequestHandlerBuilder(ILoggerFactory loggerFactory, IConfiguration configuration) : IRequestHandlerBuilder
{
    private readonly List<Rule> _rules = new();

    public int Count => _rules.Count;

    public IRequestHandler Build() => new RequestHandler(_rules, loggerFactory, configuration);

    public void AddRule(
        string ruleInfo,
        IReadOnlyCollection<IRequestMatcher> requestMatchers,
        RuleMiddlewares middlewares)
    {
        _rules.Add(new Rule(
            ruleInfo,
            requestMatchers,
            middlewares,
            loggerFactory));
    }
}