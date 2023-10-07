using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lira.Configuration;
using Lira.Domain.Configuration.Rules;
using Lira.ExternalCalling.Http.Caller;

namespace Lira.ExternalCalling.Http.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpCalling(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<LoggingHandler>();

        var builder = services.AddHttpClient(HttpExternalCaller.HttpClientName);

        if (configuration.IsLoggingEnabled())
            builder.AddHttpMessageHandler<LoggingHandler>();

        builder.ConfigurePrimaryHttpMessageHandler(provider => 
            {
                var factory = provider.GetRequiredService<IHttpMessageHandlerFactory>();
                var handler = factory.Create();
                return handler;
            });

        services.AddScoped<IExternalCallerRegistrator, HttpExternalCallerRegistrator>();
        services.AddSingleton<IHttpMessageHandlerFactory, HttpMessageHandlerFactory>();

        return services;
    }
}
