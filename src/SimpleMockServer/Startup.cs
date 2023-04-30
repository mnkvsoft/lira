using SimpleMockServer.ConfigurationProviding;
using SimpleMockServer.ExternalCalling.Http.Configuration;
using SimpleMockServer.Middlewares;
using SimpleMockServer.Configuration;

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
