using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lira.Domain;

public interface IRequestHandlerFactory
{
    IRequestHandler Create(IReadOnlyCollection<RuleData> datas);
}

class RequestHandlerFactory(ILoggerFactory loggerFactory, IConfiguration configuration) : IRequestHandlerFactory
{
    public IRequestHandler Create(IReadOnlyCollection<RuleData> datas) =>
        new RequestHandler(datas.Select(d => new Rule(d)).ToArray(), loggerFactory, configuration);
}