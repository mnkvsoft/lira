using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lira.IntegrationTests;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Replace<T>(this IServiceCollection services, T service) where T : class
    {
        services.RemoveAll(typeof(T));
        services.AddSingleton(service);
        return services;
    }
}