using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public interface IRuleExecutor
{
    IRuleMatchWeight Weight { get; }
    Rule Rule { get; }
    Task Execute(HttpResponse Response);
}
