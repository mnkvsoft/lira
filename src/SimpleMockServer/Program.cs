using SimpleMockServer;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

configuration.AddJsonFile("./appsettings.json");
configuration.AddEnvironmentVariables();

var startup = new Startup();
startup.ConfigureServices(builder.Services);
var app = builder.Build();
startup.Configure(app);

app.Run();