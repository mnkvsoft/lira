using SimpleMockServer.ConfigurationProviding;

namespace SimpleMockServer;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddLogging(x => { x.AddConsole(); })
            .AddSingleton<RoutingMiddleware>()
            .AddDomain();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<RoutingMiddleware>();
    }
}