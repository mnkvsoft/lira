using Lira.Common;
using Lira.Domain.Handling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lira.Domain;

public interface IRequestHandlerBuilder
{
    int Count { get; }
    IRequestHandler Build();

    void AddRule(string ruleInfo, IReadOnlyCollection<IRequestMatcher> requestMatchers,
        IReadOnlyCollection<Factory<Delayed<Middleware>>> middlewares);
}

public class RequestHandlerBuilder(ILoggerFactory loggerFactory, IConfiguration configuration) : IRequestHandlerBuilder
{
    private readonly List<Rule> _rules = new();

    public int Count => _rules.Count;

    public IRequestHandler Build() => new RequestHandler(_rules, loggerFactory, configuration);

    public void AddRule(
        string ruleInfo,
        IReadOnlyCollection<IRequestMatcher> requestMatchers,
        IReadOnlyCollection<Factory<Delayed<Middleware>>> middlewares)
    {
        _rules.Add(new Rule(ruleInfo, requestMatchers, middlewares));
    }
}