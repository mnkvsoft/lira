using SimpleMockServer.ExternalCalling.Http.Configuration;
using SimpleMockServer.Middlewares;
using SimpleMockServer.Configuration;
using SimpleMockServer.Domain.Configuration;
using SimpleMockServer.Domain;
using SimpleMockServer.Domain.Configuration.Rules;

namespace SimpleMockServer;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddLogging(x => { x.AddConsole(); })
            .AddSingleton<RoutingMiddleware>()
            .AddDomainConfiguration()
            .AddDomain()
            .AddHttpCalling(_configuration);
    }

    public void Configure(IApplicationBuilder app)
    { 
        if (_configuration.IsLoggingEnabled())
            app.UseMiddleware<LoggingMiddleware>();

        app.UseMiddleware<RoutingMiddleware>();
    }
}
