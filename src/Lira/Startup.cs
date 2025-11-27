using System.Diagnostics.CodeAnalysis;
using Lira.Common;
using Lira.Domain;
using Lira.Common.Exceptions;
using Lira.ExternalCalling.Http.Configuration;
using Lira.Middlewares;
using Lira.Configuration;
using Lira.Domain.Configuration;
using Lira.Domain.DataModel;

namespace Lira;

[SuppressMessage("Usage", "ASP0014:Suggest using top level route registrations")]
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
            .AddDomainConfiguration(_configuration)
            .AddDomain()
            .AddHttpCalling(_configuration)
            .AddControllers();
    }

    public void Configure(WebApplication app)
    {
        if (_configuration.IsLoggingEnabled())
            app.UseMiddleware<LoggingMiddleware>();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            string sys = _configuration.GetValue<string>("SystemEndpointStartSegment")!;

            endpoints.MapGet(
                "/" + sys + "/range/val/{name}/{rangeName}/{count:int?}",
                async (HttpContext context, string name, string rangeName, int? count) =>
                {
                    var dataProvider = context.RequestServices.GetRequiredService<IRangesProvider>();

                    var data = dataProvider.Find(new DataName(name));

                    if (data == null)
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync($"Range '{name}' not found");
                    }
                    else
                    {
                        var range = data.Find(new DataName(rangeName));

                        if (range == null)
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync($"Range '{rangeName}' in interval '{name}' not found");
                        }
                        else
                        {
                            context.Response.StatusCode = 200;
                            count ??= 20;
                            for (int i = 0; i < count; i++)
                            {
                                object nextValue = range.NextValue();
                                await context.Response.WriteAsync((nextValue.ToString() ?? "") + ((i == count - 1) ? "" : Constants.NewLine));
                            }
                        }
                    }
                });

            endpoints.MapGet(
              "/" + sys + "/range/info",
              async context =>
              {
                  var dataProvider = context.RequestServices.GetRequiredService<IRangesProvider>();
                  var datas = dataProvider.GetAll();
                  await WriteRanges(context, datas);
              });

            endpoints.MapGet(
            "/" + sys + "/range/info/{name}",
            async (HttpContext context, string name) =>
            {
                var dataProvider = context.RequestServices.GetRequiredService<IRangesProvider>();

                var data = dataProvider.Find(new DataName(name));
                if(data == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync($"Data '{name}' not found");
                }
                else
                {
                    await WriteRanges(context, [data]);
                }
            });

            endpoints.MapGet(
            "/" + sys + "/state",
            async context =>
            {
                var loader = context.RequestServices.GetRequiredService<IConfigurationLoader>();

                var state = await loader.GetState();

                if (state is ConfigurationState.Ok ok)
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync($"Ok. Load time: {ok.LoadTime} " +
                                                      $"({(int)(DateTime.Now - ok.LoadTime).TotalSeconds} second ago)");
                }
                else if(state is ConfigurationState.Error error)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"Error. Load time: {error.LoadTime} " +
                                                      $"({(int)(DateTime.Now - error.LoadTime).TotalSeconds} second ago)" +
                                                      Constants.NewLine + Constants.NewLine +
                                                      error.Exception);
                }
                else
                {
                    throw new UnsupportedInstanceType(state);
                }
            });
        });

        app.UseMiddleware<RoutingMiddleware>();
    }

    private static async Task WriteRanges(HttpContext context, IReadOnlyCollection<Data> datas)
    {
        var nl = Constants.NewLine;
        context.Response.StatusCode = 200;

        foreach (var data in datas)
        {
            await context.Response.WriteAsync("-------- " + data.Name
                + nl + nl +
                data.Info
                + nl + nl + nl + nl);
        }
    }
}