using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SimpleMockServer.IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Startup>
{
    private readonly string _searchPath;
    private readonly AppMocks? _appMocks;


    public TestApplicationFactory(string searchPath, AppMocks? appMocks = null)
    {
        _searchPath = searchPath;
        _appMocks = appMocks;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var settings = new Dictionary<string, string?>
        {
            {"RulesPath", _searchPath},
        };

        var cfgBuilder = new ConfigurationBuilder();
        cfgBuilder.AddInMemoryCollection(settings);
        IConfiguration cfg = cfgBuilder.Build();

        builder.UseConfiguration(cfg);
        builder.ConfigureLogging(x =>
        {
            x.SetMinimumLevel(LogLevel.Information);
            x.AddConsole();
        });
        builder.ConfigureTestServices(services => _appMocks?.Configure(services));
    }
}
