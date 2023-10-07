using System.Globalization;
using Lira;
using Lira.Domain.Configuration;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var startup = new Startup(configuration);
startup.ConfigureServices(builder.Services);
var app = builder.Build();

startup.Configure(app);

var loader = app.Services.GetRequiredService<IConfigurationLoader>();
loader.ProvokeLoad();

app.Run();
