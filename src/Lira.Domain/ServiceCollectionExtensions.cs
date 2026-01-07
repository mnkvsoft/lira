using Lira.Common;
using Lira.Domain.Handling.Generating.History;
using Lira.Domain.Handling.Generating.ResponseStrategies;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;
using Lira.Domain.Matching.Conditions;
using Microsoft.Extensions.DependencyInjection;

namespace Lira.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services
            .AddSingleton<ResponseCache>()
            .AddSingleton<ResponseMiddlewareFactory>()
            .AddSingleton<IMiddlewareFactory, MiddlewareFactory>()
            .AddSingleton<HandledRuleHistoryStorage>()
            .AddSingleton<IHandledRuleHistoryStorage>(provider => provider.GetRequiredService<HandledRuleHistoryStorage>())
            .AddSingleton<IRequestStatisticStorage, RequestStatisticStorage>()
            .AddTransient<RequestHandlerBuilder>()
            .AddSingleton<Factory<IRequestHandlerBuilder>>(provider => provider.GetRequiredService<RequestHandlerBuilder>);
    }
}
