using System.Globalization;
using SimpleMockServer;
using SimpleMockServer.Domain.Configuration.Rules;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

//configuration.AddJsonFile("./appsettings.json");
//configuration.AddEnvironmentVariables();

var startup = new Startup(configuration);
startup.ConfigureServices(builder.Services);
var app = builder.Build();

startup.Configure(app);

var providers = app.Services.GetRequiredService<IEnumerable<IStatedProvider>>();
foreach (var provider in providers)
{
    // begin loading configuration
    provider.GetState();
}

app.Run();
