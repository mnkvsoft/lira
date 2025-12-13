using System.Globalization;
using System.Reflection;
using Lira.Common;
using Lira.Configuration;
using Lira.Domain;
using Lira.Domain.Configuration;
using Lira.ExternalCalling.Http.Configuration;
using Lira.Middlewares;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
     // swagger
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
     // other
    .AddLogging(x => { x.AddConsole(); })
    .AddSingleton<RoutingMiddleware>()
    .AddDomainConfiguration(configuration)
    .AddDomain()
    .AddHttpCalling(configuration)
    .AddControllers();

var app = builder.Build();


if (configuration.IsLoggingEnabled())
    app.UseMiddleware<LoggingMiddleware>();

app.UseMiddleware<RoutingMiddleware>();

app.UsePathBase(configuration.GetValue<string>("SystemEndpointStartSegment"));
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.MapControllers();

var loader = app.Services.GetRequiredService<IConfigurationLoader>();
loader.BeginLoading();
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Application");
logger.LogInformation("App version: " + Assembly.GetEntryAssembly()!.GetName().Version);
logger.LogInformation("Temp files path: " + Paths.GetTempPath);

app.Run();

public partial class Program;