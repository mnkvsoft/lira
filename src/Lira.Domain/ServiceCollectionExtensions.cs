using Lira.Domain.Handling.Generating;
using Lira.Domain.Handling.Generating.History;
using Lira.Domain.Handling.Generating.ResponseStrategies;
using Lira.Domain.Matching.Conditions;
using Microsoft.Extensions.DependencyInjection;

namespace Lira.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services
            .AddSingleton<IResponseGenerationHandlerFactory, ResponseGenerationHandlerFactory>()
            .AddSingleton<HandledRuleHistoryStorage>()
            .AddSingleton<IHandledRuleHistoryStorage>(provider => provider.GetRequiredService<HandledRuleHistoryStorage>())
            .AddSingleton<IRequestStatisticStorage, RequestStatisticStorage>()
            .AddSingleton<IRequestHandlerFactory, RequestHandlerFactory>();
    }
}
