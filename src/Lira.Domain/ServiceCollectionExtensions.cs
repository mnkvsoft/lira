using Lira.Domain.Matching.Conditions;
using Microsoft.Extensions.DependencyInjection;

namespace Lira.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services.AddSingleton<IRequestStatisticStorage, RequestStatisticStorage>();
    }
}
