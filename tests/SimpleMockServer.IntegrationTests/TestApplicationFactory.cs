using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SimpleMockServer.IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Startup>
{
    private readonly string _searchPath;

    public TestApplicationFactory(string searchPath)
    {
        _searchPath = searchPath;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var settings = new Dictionary<string, string>
        {
            {"SEARCH_PATH", _searchPath},
        };

        var cfgBuilder = new ConfigurationBuilder();
        cfgBuilder.AddInMemoryCollection(settings);
        IConfiguration cfg = cfgBuilder.Build();

        builder.UseConfiguration(cfg);
        builder.ConfigureLogging(x =>
        {
            //x.SetMinimumLevel(LogLevel.Warning);
            //x.AddConsole();
        });
    }

    public HttpClient CreateHttpClient()
    {
        return CreateDefaultClient(new LoggingHandler());
    }
}